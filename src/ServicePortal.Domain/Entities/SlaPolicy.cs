using System.ComponentModel.DataAnnotations;
using ServicePortal.Domain.Enums;

namespace ServicePortal.Domain.Entities;

/// <summary>
/// Defines SLA response and resolution targets by priority level.
/// Targets are in business hours (Mon-Fri 09:00-18:00).
/// </summary>
public class SlaPolicy
{
    public int Id { get; set; }

    public RequestPriority Priority { get; set; }

    /// <summary>Response target in business hours.</summary>
    public int ResponseTargetHours { get; set; }

    /// <summary>Resolution target in business hours.</summary>
    public int ResolutionTargetHours { get; set; }

    public bool IsActive { get; set; } = true;
}
