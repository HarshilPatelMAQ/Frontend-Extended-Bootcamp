namespace ServicePortal.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a service request.
/// Workflow: New -> Assigned -> InProgress -> WaitingForRequester -> Resolved -> Closed
/// </summary>
public enum RequestStatus
{
    New = 0,
    Assigned = 1,
    InProgress = 2,
    WaitingForRequester = 3,
    Resolved = 4,
    Closed = 5
}
