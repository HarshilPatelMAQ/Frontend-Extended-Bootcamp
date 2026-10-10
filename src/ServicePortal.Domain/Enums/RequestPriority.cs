namespace ServicePortal.Domain.Enums;

/// <summary>
/// Priority levels for service requests, used for SLA calculations.
/// </summary>
public enum RequestPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
