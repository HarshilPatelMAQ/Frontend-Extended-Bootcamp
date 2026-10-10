namespace ServicePortal.Domain.Enums;

/// <summary>
/// Types of notifications sent to users.
/// </summary>
public enum NotificationType
{
    RequestCreated = 0,
    RequestAssigned = 1,
    RequestCommented = 2,
    RequestResolved = 3,
    RequestReopened = 4,
    RequestClosed = 5,
    SlaBreach = 6,
    SlaWarning = 7,
    StatusChanged = 8
}
