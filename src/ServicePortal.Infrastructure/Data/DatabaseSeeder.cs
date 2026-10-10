using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServicePortal.Domain.Entities;
using ServicePortal.Domain.Enums;
using ServicePortal.Infrastructure.Identity;

namespace ServicePortal.Infrastructure.Data;

/// <summary>
/// Seeds the database with demo data: roles, users, teams, categories, SLA policies,
/// and sample service requests. Uses synthetic data only.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // Seed roles
        string[] roles = { "Administrator", "TeamLead", "SupportAgent", "Requester" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed support teams
        if (!await context.SupportTeams.AnyAsync())
        {
            var teams = new List<SupportTeam>
            {
                new() { Name = "IT Support", Description = "Information Technology support and troubleshooting", IsActive = true },
                new() { Name = "HR Services", Description = "Human Resources requests and inquiries", IsActive = true },
                new() { Name = "Facilities", Description = "Building and facility maintenance", IsActive = true },
                new() { Name = "General Support", Description = "General inquiries and support requests", IsActive = true }
            };
            context.SupportTeams.AddRange(teams);
            await context.SaveChangesAsync();
        }

        var itTeam = await context.SupportTeams.FirstAsync(t => t.Name == "IT Support");
        var hrTeam = await context.SupportTeams.FirstAsync(t => t.Name == "HR Services");
        var facilitiesTeam = await context.SupportTeams.FirstAsync(t => t.Name == "Facilities");
        var generalTeam = await context.SupportTeams.FirstAsync(t => t.Name == "General Support");

        // Seed categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "Hardware Issue", SupportTeamId = itTeam.Id, IsActive = true },
                new() { Name = "Software Issue", SupportTeamId = itTeam.Id, IsActive = true },
                new() { Name = "Network Problem", SupportTeamId = itTeam.Id, IsActive = true },
                new() { Name = "Access Request", SupportTeamId = itTeam.Id, IsActive = true },
                new() { Name = "Leave Request", SupportTeamId = hrTeam.Id, IsActive = true },
                new() { Name = "Benefits Inquiry", SupportTeamId = hrTeam.Id, IsActive = true },
                new() { Name = "Office Maintenance", SupportTeamId = facilitiesTeam.Id, IsActive = true },
                new() { Name = "Meeting Room", SupportTeamId = facilitiesTeam.Id, IsActive = true },
                new() { Name = "General Inquiry", SupportTeamId = generalTeam.Id, IsActive = true },
                new() { Name = "Feedback", SupportTeamId = generalTeam.Id, IsActive = true }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        // Seed SLA policies
        if (!await context.SlaPolicies.AnyAsync())
        {
            var policies = new List<SlaPolicy>
            {
                new() { Priority = RequestPriority.Critical, ResponseTargetHours = 1, ResolutionTargetHours = 4, IsActive = true },
                new() { Priority = RequestPriority.High, ResponseTargetHours = 4, ResolutionTargetHours = 8, IsActive = true },
                new() { Priority = RequestPriority.Medium, ResponseTargetHours = 8, ResolutionTargetHours = 24, IsActive = true },
                new() { Priority = RequestPriority.Low, ResponseTargetHours = 24, ResolutionTargetHours = 72, IsActive = true }
            };
            context.SlaPolicies.AddRange(policies);
            await context.SaveChangesAsync();
        }

        // Seed users
        await SeedUserAsync(userManager, "admin@contoso.com", "Admin@123", "System Administrator", "Administrator", null);

        await SeedUserAsync(userManager, "it.lead@contoso.com", "Lead@123", "IT Team Lead", "TeamLead", itTeam.Id);
        await SeedUserAsync(userManager, "hr.lead@contoso.com", "Lead@123", "HR Team Lead", "TeamLead", hrTeam.Id);

        await SeedUserAsync(userManager, "agent1@contoso.com", "Agent@123", "IT Agent Alice", "SupportAgent", itTeam.Id);
        await SeedUserAsync(userManager, "agent2@contoso.com", "Agent@123", "IT Agent Bob", "SupportAgent", itTeam.Id);
        await SeedUserAsync(userManager, "agent3@contoso.com", "Agent@123", "HR Agent Carol", "SupportAgent", hrTeam.Id);

        await SeedUserAsync(userManager, "requester1@contoso.com", "User@123", "John Employee", "Requester", null);
        await SeedUserAsync(userManager, "requester2@contoso.com", "User@123", "Jane Employee", "Requester", null);

        // Seed demo service requests
        if (!await context.ServiceRequests.AnyAsync())
        {
            var requester1 = await userManager.FindByEmailAsync("requester1@contoso.com");
            var requester2 = await userManager.FindByEmailAsync("requester2@contoso.com");
            var agent1 = await userManager.FindByEmailAsync("agent1@contoso.com");
            var softwareCategory = await context.Categories.FirstAsync(c => c.Name == "Software Issue");
            var hardwareCategory = await context.Categories.FirstAsync(c => c.Name == "Hardware Issue");
            var leaveCategory = await context.Categories.FirstAsync(c => c.Name == "Leave Request");

            var requests = new List<ServiceRequest>
            {
                new()
                {
                    ReferenceNumber = "SR-2026-000001",
                    Title = "Cannot access company email",
                    Description = "I am unable to log into my company email account since this morning. Getting 'authentication failed' error.",
                    CategoryId = softwareCategory.Id,
                    Priority = RequestPriority.High,
                    Status = RequestStatus.Assigned,
                    RequesterId = requester1!.Id,
                    AssigneeId = agent1!.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-2),
                    AssignedDate = DateTime.UtcNow.AddDays(-2).AddHours(1),
                    LastUpdatedDate = DateTime.UtcNow.AddDays(-1),
                    ResponseDueDate = DateTime.UtcNow.AddDays(-1),
                    ResolutionDueDate = DateTime.UtcNow.AddDays(1),
                    RowVersion = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
                },
                new()
                {
                    ReferenceNumber = "SR-2026-000002",
                    Title = "Laptop screen flickering",
                    Description = "My laptop screen has been flickering intermittently for the past week. It happens especially when running multiple applications.",
                    CategoryId = hardwareCategory.Id,
                    Priority = RequestPriority.Medium,
                    Status = RequestStatus.New,
                    RequesterId = requester2!.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-1),
                    LastUpdatedDate = DateTime.UtcNow.AddDays(-1),
                    ResponseDueDate = DateTime.UtcNow.AddDays(1),
                    ResolutionDueDate = DateTime.UtcNow.AddDays(5),
                    RowVersion = BitConverter.GetBytes(DateTime.UtcNow.Ticks + 1)
                },
                new()
                {
                    ReferenceNumber = "SR-2026-000003",
                    Title = "Annual leave request",
                    Description = "Requesting 5 days annual leave from Nov 1 to Nov 5, 2026 for personal vacation.",
                    CategoryId = leaveCategory.Id,
                    Priority = RequestPriority.Low,
                    Status = RequestStatus.Resolved,
                    RequesterId = requester1.Id,
                    ResolutionSummary = "Leave request approved by HR manager.",
                    ResolutionCategoryValue = ResolutionCategory.Fixed,
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                    ResolvedDate = DateTime.UtcNow.AddDays(-7),
                    LastUpdatedDate = DateTime.UtcNow.AddDays(-7),
                    ResponseDueDate = DateTime.UtcNow.AddDays(-8),
                    ResolutionDueDate = DateTime.UtcNow.AddDays(-3),
                    RowVersion = BitConverter.GetBytes(DateTime.UtcNow.Ticks + 2)
                }
            };

            context.ServiceRequests.AddRange(requests);
            await context.SaveChangesAsync();

            // Add status history for demo requests
            var sr1 = await context.ServiceRequests.FirstAsync(r => r.ReferenceNumber == "SR-2026-000001");
            context.RequestStatusHistory.AddRange(
                new RequestStatusHistory
                {
                    ServiceRequestId = sr1.Id, FromStatus = RequestStatus.New, ToStatus = RequestStatus.Assigned,
                    ChangedById = "system", Reason = "Auto-assigned to IT Support team", ChangedDate = sr1.CreatedDate.AddHours(1)
                }
            );

            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string password, string displayName,
        string role, int? teamId)
    {
        if (await userManager.FindByEmailAsync(email) != null)
            return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            IsActive = true,
            SupportTeamId = teamId,
            EmailConfirmed = true // Pre-verified for demo
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }
}
