using System.ComponentModel.DataAnnotations;
using ServicePortal.Domain.Enums;

namespace ServicePortal.Domain.Entities;

/// <summary>
/// Core entity representing a service request submitted by a user.
/// Uses a concurrency token (RowVersion) to prevent silent overwrites.
/// Demonstrates OOP concepts: encapsulation, composition, and relationships.
/// </summary>
public class ServiceRequest
{
    public int Id { get; set; }

    /// <summary>
    /// Human-readable reference number, e.g., SR-2026-000123.
    /// Generated safely under concurrent requests.
    /// </summary>
    [Required, MaxLength(20)]
    public string ReferenceNumber { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public RequestPriority Priority { get; set; } = RequestPriority.Medium;

    public RequestStatus Status { get; set; } = RequestStatus.New;

    [Required]
    public string RequesterId { get; set; } = string.Empty;

    public string? AssigneeId { get; set; }

    [MaxLength(50)]
    public string PreferredContactMethod { get; set; } = "Email";

    // SLA tracking
    public DateTime? ResponseDueDate { get; set; }
    public DateTime? ResolutionDueDate { get; set; }
    public bool IsResponseSlaBreached { get; set; }
    public bool IsResolutionSlaBreached { get; set; }

    // Resolution data
    [MaxLength(2000)]
    public string? ResolutionSummary { get; set; }
    public ResolutionCategory? ResolutionCategoryValue { get; set; }

    // Dates
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? AssignedDate { get; set; }
    public DateTime? FirstResponseDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Concurrency token to prevent silent overwrites (AC-10).
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public virtual Category? Category { get; set; }
    public virtual ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();
    public virtual ICollection<RequestAttachment> Attachments { get; set; } = new List<RequestAttachment>();
    public virtual ICollection<RequestStatusHistory> StatusHistory { get; set; } = new List<RequestStatusHistory>();
    public virtual ICollection<AssignmentHistory> AssignmentHistory { get; set; } = new List<AssignmentHistory>();
}
