using System.ComponentModel.DataAnnotations;

namespace ServicePortal.Domain.Entities;

/// <summary>
/// Audit log entry for tracking all significant actions in the system.
/// Includes correlation identifiers for troubleshooting (AC-11).
/// </summary>
public class AuditLog
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Actor { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? EntityId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(2000)]
    public string? ChangeSummary { get; set; }

    /// <summary>
    /// Correlation identifier for linking related log entries.
    /// </summary>
    [Required, MaxLength(50)]
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
