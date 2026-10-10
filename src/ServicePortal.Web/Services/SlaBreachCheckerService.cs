using Microsoft.EntityFrameworkCore;
using ServicePortal.Domain.Entities;
using ServicePortal.Domain.Enums;
using ServicePortal.Infrastructure.Data;

namespace ServicePortal.Web.Services;

/// <summary>
/// Background service that checks for SLA breaches periodically.
/// Creates audit entries and notifications when SLA is breached (AC-06).
/// </summary>
public class SlaBreachCheckerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SlaBreachCheckerService> _logger;

    public SlaBreachCheckerService(IServiceProvider serviceProvider, ILogger<SlaBreachCheckerService> logger)
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
                await CheckSlaBreaches();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking SLA breaches. CorrelationId: {CorrelationId}", Guid.NewGuid());
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task CheckSlaBreaches()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTime.UtcNow;

        // Check response SLA breaches
        var responseBreaches = await context.ServiceRequests
            .Where(r => r.Status != RequestStatus.Closed &&
                        r.Status != RequestStatus.Resolved &&
                        r.ResponseDueDate.HasValue &&
                        r.ResponseDueDate < now &&
                        !r.IsResponseSlaBreached)
            .ToListAsync();

        foreach (var request in responseBreaches)
        {
            request.IsResponseSlaBreached = true;
            var correlationId = Guid.NewGuid().ToString();

            context.AuditLogs.Add(new AuditLog
            {
                Actor = "System",
                Action = "SLA_RESPONSE_BREACH",
                EntityType = "ServiceRequest",
                EntityId = request.Id.ToString(),
                ChangeSummary = $"Response SLA breached for {request.ReferenceNumber}",
                CorrelationId = correlationId
            });

            if (!string.IsNullOrEmpty(request.AssigneeId))
            {
                context.Notifications.Add(new Notification
                {
                    RecipientId = request.AssigneeId,
                    Type = NotificationType.SlaBreach,
                    Message = $"Response SLA breached for request {request.ReferenceNumber}",
                    ServiceRequestId = request.Id
                });
            }

            _logger.LogWarning("Response SLA breached for {Reference}. CorrelationId: {CorrelationId}",
                request.ReferenceNumber, correlationId);
        }

        // Check resolution SLA breaches
        var resolutionBreaches = await context.ServiceRequests
            .Where(r => r.Status != RequestStatus.Closed &&
                        r.Status != RequestStatus.Resolved &&
                        r.ResolutionDueDate.HasValue &&
                        r.ResolutionDueDate < now &&
                        !r.IsResolutionSlaBreached)
            .ToListAsync();

        foreach (var request in resolutionBreaches)
        {
            request.IsResolutionSlaBreached = true;
            var correlationId = Guid.NewGuid().ToString();

            context.AuditLogs.Add(new AuditLog
            {
                Actor = "System",
                Action = "SLA_RESOLUTION_BREACH",
                EntityType = "ServiceRequest",
                EntityId = request.Id.ToString(),
                ChangeSummary = $"Resolution SLA breached for {request.ReferenceNumber}",
                CorrelationId = correlationId
            });

            _logger.LogWarning("Resolution SLA breached for {Reference}. CorrelationId: {CorrelationId}",
                request.ReferenceNumber, correlationId);
        }

        if (responseBreaches.Count > 0 || resolutionBreaches.Count > 0)
            await context.SaveChangesAsync();
    }
}
