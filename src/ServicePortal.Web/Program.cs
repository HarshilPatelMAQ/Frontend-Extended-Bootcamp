using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServicePortal.Application.Services;
using ServicePortal.Domain.Interfaces;
using ServicePortal.Infrastructure.Data;
using ServicePortal.Infrastructure.Identity;
using ServicePortal.Infrastructure.Services;
using ServicePortal.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ───────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost\\SQLEXPRESS;Database=ServicePortalDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ─── Identity with roles ────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// ─── Authorization Policies ─────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministrator", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("RequireTeamLead", policy => policy.RequireRole("TeamLead", "Administrator"));
    options.AddPolicy("RequireAgent", policy => policy.RequireRole("SupportAgent", "TeamLead", "Administrator"));
    options.AddPolicy("RequireStaff", policy => policy.RequireRole("SupportAgent", "TeamLead", "Administrator"));
});

// ─── Application Services (Dependency Injection) ───────────────────────
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<ISlaCalculationService, SlaCalculationService>();
builder.Services.AddScoped<IEmailService, MockEmailService>();

// ─── Background Services ───────────────────────────────────────────────
builder.Services.AddHostedService<SlaBreachCheckerService>();
builder.Services.AddHostedService<AutoCloseResolvedRequestsService>();

// ─── MVC ────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ─── Seed Database ──────────────────────────────────────────────────────
await DatabaseSeeder.SeedAsync(app.Services);

// ─── Pipeline ───────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
