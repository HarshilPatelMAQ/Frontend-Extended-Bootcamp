using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ServicePortal.Infrastructure.Identity;

/// <summary>
/// Extended Identity user with additional fields required by the portal.
/// Inherits from IdentityUser demonstrating Inheritance (OOP concept).
/// </summary>
public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user account is active. Inactive users are denied access (AC-02).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The support team this user belongs to (for agents and team leads).
    /// </summary>
    public int? SupportTeamId { get; set; }
}
