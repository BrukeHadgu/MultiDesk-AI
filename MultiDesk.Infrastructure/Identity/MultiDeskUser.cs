using Microsoft.AspNetCore.Identity;

namespace MultiDesk.Infrastructure.Identity;

public class MultiDeskUser : IdentityUser
{
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public string? Department { get; set; }
  public int TenantId { get; set; } = 1;
  public bool IsActive { get; set; } = true;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // Refresh token fields
  public string? RefreshToken { get; set; }
  public DateTime? RefreshTokenExpiry { get; set; }

  public string FullName => $"{FirstName} {LastName}";
}