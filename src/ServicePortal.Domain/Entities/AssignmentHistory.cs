using System.ComponentModel.DataAnnotations;

namespace ServicePortal.Domain.Entities;

/// <summary>
/// Records assignment/reassignment history for audit and tracking.
/// </summary>
public class AssignmentHistory
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }

    public string? PreviousAssigneeId { get; set; }

    public string? NewAssigneeId { get; set; }

    [Required]
    public string AssignedById { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; set; }

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ServiceRequest? ServiceRequest { get; set; }
}
