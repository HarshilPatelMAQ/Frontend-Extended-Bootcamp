using System.ComponentModel.DataAnnotations;
using ServicePortal.Domain.Enums;

namespace ServicePortal.Domain.Entities;

/// <summary>
/// Audit trail for status transitions. Every transition is validated and recorded (AC-04).
/// </summary>
public class RequestStatusHistory
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }

    public RequestStatus FromStatus { get; set; }
    public RequestStatus ToStatus { get; set; }

    [Required]
    public string ChangedById { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Reason { get; set; }

    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ServiceRequest? ServiceRequest { get; set; }
}
