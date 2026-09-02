using Microsoft.AspNetCore.Identity;

namespace MultiDesk.Infrastructure.Identity;

public class MultiDeskUser : IdentityUser
{
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public int? DepartmentId { get; set; }
  public int TenantId { get; set; } = 1;
  public bool IsActive { get; set; } = true;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // Forces password change on next login
  // Set to true when admin or SuperAdmin creates the account
  public bool MustChangePassword { get; set; } = false;

  // Refresh token fields
  public string? RefreshToken { get; set; }
  public DateTime? RefreshTokenExpiry { get; set; }
  public string TenantUserId { get; set; } = string.Empty;
  public string FullName => $"{FirstName} {LastName}";
}