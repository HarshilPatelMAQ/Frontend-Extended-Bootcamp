using ServicePortal.Domain.Enums;
using ServicePortal.Domain.Interfaces;

namespace ServicePortal.Application.Services;

/// <summary>
/// Validates request workflow transitions against business rules.
/// Demonstrates Polymorphism through interface implementation and
/// Encapsulation by hiding transition logic details.
/// </summary>
public class WorkflowService : IWorkflowService
{
    // Valid transitions map: from status -> allowed target statuses
    private static readonly Dictionary<RequestStatus, List<RequestStatus>> ValidTransitions = new()
    {
        { RequestStatus.New, new List<RequestStatus> { RequestStatus.Assigned } },
        { RequestStatus.Assigned, new List<RequestStatus> { RequestStatus.InProgress } },
        { RequestStatus.InProgress, new List<RequestStatus> { RequestStatus.WaitingForRequester, RequestStatus.Resolved } },
        { RequestStatus.WaitingForRequester, new List<RequestStatus> { RequestStatus.InProgress, RequestStatus.Resolved } },
        { RequestStatus.Resolved, new List<RequestStatus> { RequestStatus.Closed, RequestStatus.InProgress } }, // InProgress = reopen
        { RequestStatus.Closed, new List<RequestStatus>() } // Closed cannot transition
    };

    /// <inheritdoc />
    public bool IsValidTransition(RequestStatus from, RequestStatus to)
    {
        return ValidTransitions.TryGetValue(from, out var validTargets) && validTargets.Contains(to);
    }

    /// <inheritdoc />
    public IReadOnlyList<RequestStatus> GetValidTransitions(RequestStatus currentStatus)
    {
        return ValidTransitions.TryGetValue(currentStatus, out var transitions)
            ? transitions.AsReadOnly()
            : new List<RequestStatus>().AsReadOnly();
    }
}
