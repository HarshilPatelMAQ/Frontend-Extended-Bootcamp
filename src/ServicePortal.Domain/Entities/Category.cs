using System.ComponentModel.DataAnnotations;

namespace ServicePortal.Domain.Entities;

/// <summary>
/// Service request category mapped to a specific support team.
/// New requests are automatically routed based on their category's team.
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SupportTeamId { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual SupportTeam? SupportTeam { get; set; }
}
