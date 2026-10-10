using System.ComponentModel.DataAnnotations;
using ServicePortal.Domain.Enums;

namespace ServicePortal.Domain.Entities;

/// <summary>
/// In-app notification for users. Created on key events (assignment, comment, SLA breach, etc.).
/// </summary>
public class Notification
{
    public int Id { get; set; }

    [Required]
    public string RecipientId { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public int? ServiceRequestId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ServiceRequest? ServiceRequest { get; set; }
}
