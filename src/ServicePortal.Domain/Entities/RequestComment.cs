using System.ComponentModel.DataAnnotations;

namespace ServicePortal.Domain.Entities;

/// <summary>
/// Comment on a service request. Can be public (visible to requester) or internal (staff only).
/// Comments are immutable - corrections must be added as new comments (AC-03).
/// </summary>
public class RequestComment
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }

    [Required]
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>
    /// If true, only visible to agents/leads/admins. Never shown to requesters.
    /// </summary>
    public bool IsInternal { get; set; }

    [Required, MaxLength(5000)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ServiceRequest? ServiceRequest { get; set; }
    public virtual ICollection<RequestAttachment> Attachments { get; set; } = new List<RequestAttachment>();
}
