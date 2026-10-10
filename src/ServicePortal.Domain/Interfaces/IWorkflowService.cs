using ServicePortal.Domain.Enums;

namespace ServicePortal.Domain.Interfaces;

/// <summary>
/// Interface for validating workflow transitions. Demonstrates Interface Segregation (SOLID).
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Checks if a status transition is valid according to business rules.
    /// </summary>
    bool IsValidTransition(RequestStatus from, RequestStatus to);

    /// <summary>
    /// Gets all valid next statuses from the current status.
    /// </summary>
    IReadOnlyList<RequestStatus> GetValidTransitions(RequestStatus currentStatus);
}
