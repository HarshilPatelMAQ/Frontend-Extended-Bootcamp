using System.ComponentModel.DataAnnotations;

namespace ServicePortal.Domain.Entities;

/// <summary>
/// File attachment associated with a service request or comment.
/// Files are stored outside the public web root for security.
/// </summary>
public class RequestAttachment
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }
    public int? CommentId { get; set; }

    [Required, MaxLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [Required]
    public string UploaderId { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ServiceRequest? ServiceRequest { get; set; }
    public virtual RequestComment? Comment { get; set; }
}
