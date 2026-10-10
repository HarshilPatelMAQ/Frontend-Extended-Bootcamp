using Microsoft.EntityFrameworkCore;
using ServicePortal.Domain.Entities;
using ServicePortal.Domain.Enums;
using ServicePortal.Infrastructure.Data;

namespace ServicePortal.Web.Services;

/// <summary>
/// Background service that auto-closes resolved requests after 7 calendar days (AC-07).
/// Records an automated history entry for audit trail.
/// </summary>
public class AutoCloseResolvedRequestsService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoCloseResolvedRequestsService> _logger;

    public AutoCloseResolvedRequestsService(IServiceProvider serviceProvider, ILogger<AutoCloseResolvedRequestsService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AutoCloseRequests();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-closing resolved requests. CorrelationId: {CorrelationId}", Guid.NewGuid());
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task AutoCloseRequests()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cutoff = DateTime.UtcNow.AddDays(-7);

        var eligibleRequests = await context.ServiceRequests
            .Where(r => r.Status == RequestStatus.Resolved &&
                        r.ResolvedDate.HasValue &&
                        r.ResolvedDate <= cutoff)
            .ToListAsync();

        foreach (var request in eligibleRequests)
        {
            var correlationId = Guid.NewGuid().ToString();

            request.Status = RequestStatus.Closed;
            request.ClosedDate = DateTime.UtcNow;
            request.LastUpdatedDate = DateTime.UtcNow;

            context.RequestStatusHistory.Add(new RequestStatusHistory
            {
                ServiceRequestId = request.Id,
                FromStatus = RequestStatus.Resolved,
                ToStatus = RequestStatus.Closed,
                ChangedById = "system-autoclose",
                Reason = "Automatically closed after 7 days in Resolved status",
                ChangedDate = DateTime.UtcNow
            });

            context.AuditLogs.Add(new AuditLog
            {
                Actor = "System",
                Action = "AUTO_CLOSE",
                EntityType = "ServiceRequest",
                EntityId = request.Id.ToString(),
                ChangeSummary = $"Request {request.ReferenceNumber} auto-closed after 7 days in Resolved status",
                CorrelationId = correlationId
            });

            // Notify requester
            context.Notifications.Add(new Notification
            {
                RecipientId = request.RequesterId,
                Type = NotificationType.RequestClosed,
                Message = $"Your request {request.ReferenceNumber} has been automatically closed.",
                ServiceRequestId = request.Id
            });

            _logger.LogInformation("Auto-closed request {Reference}. CorrelationId: {CorrelationId}",
                request.ReferenceNumber, correlationId);
        }

        if (eligibleRequests.Count > 0)
            await context.SaveChangesAsync();
    }
}
