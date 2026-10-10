using System.ComponentModel.DataAnnotations;

namespace ServicePortal.Domain.Entities;

/// <summary>
/// Represents a support team that handles service requests for specific categories.
/// Demonstrates encapsulation - properties are validated through data annotations.
/// </summary>
public class SupportTeam
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
