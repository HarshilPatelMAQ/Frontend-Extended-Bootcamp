using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServicePortal.Domain.Entities;
using ServicePortal.Infrastructure.Identity;

namespace ServicePortal.Infrastructure.Data;

/// <summary>
/// Application database context. Uses EF Core Code-First with Identity integration.
/// Demonstrates separation of concerns by keeping data access in the Infrastructure layer.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<SupportTeam> SupportTeams => Set<SupportTeam>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<RequestComment> RequestComments => Set<RequestComment>();
    public DbSet<RequestAttachment> RequestAttachments => Set<RequestAttachment>();
    public DbSet<RequestStatusHistory> RequestStatusHistory => Set<RequestStatusHistory>();
    public DbSet<AssignmentHistory> AssignmentHistory => Set<AssignmentHistory>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ServiceRequest configuration
        builder.Entity<ServiceRequest>(entity =>
        {
            entity.HasIndex(e => e.ReferenceNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequesterId);
            entity.HasIndex(e => e.AssigneeId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.CreatedDate);

            // Concurrency token for SQLite (use RowVersion as manual check)
            entity.Property(e => e.RowVersion).IsConcurrencyToken();

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Category -> SupportTeam
        builder.Entity<Category>(entity =>
        {
            entity.HasOne(e => e.SupportTeam)
                .WithMany(t => t.Categories)
                .HasForeignKey(e => e.SupportTeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RequestComment
        builder.Entity<RequestComment>(entity =>
        {
            entity.HasOne(e => e.ServiceRequest)
                .WithMany(r => r.Comments)
                .HasForeignKey(e => e.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RequestAttachment
        builder.Entity<RequestAttachment>(entity =>
        {
            entity.HasOne(e => e.ServiceRequest)
                .WithMany(r => r.Attachments)
                .HasForeignKey(e => e.ServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Comment)
                .WithMany(c => c.Attachments)
                .HasForeignKey(e => e.CommentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // RequestStatusHistory
        builder.Entity<RequestStatusHistory>(entity =>
        {
            entity.HasOne(e => e.ServiceRequest)
                .WithMany(r => r.StatusHistory)
                .HasForeignKey(e => e.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ServiceRequestId);
        });

        // AssignmentHistory
        builder.Entity<AssignmentHistory>(entity =>
        {
            entity.HasOne(e => e.ServiceRequest)
                .WithMany(r => r.AssignmentHistory)
                .HasForeignKey(e => e.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Notification
        builder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.RecipientId);
            entity.HasIndex(e => e.IsRead);

            entity.HasOne(e => e.ServiceRequest)
                .WithMany()
                .HasForeignKey(e => e.ServiceRequestId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // AuditLog
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => e.CorrelationId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EntityType);
        });

        // SlaPolicy
        builder.Entity<SlaPolicy>(entity =>
        {
            entity.HasIndex(e => e.Priority);
        });
    }
}
