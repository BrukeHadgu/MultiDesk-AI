namespace MultiDesk.Domain.Entities;

public class RefreshToken
{
  public int Id { get; set; }
  public string Token { get; set; } = string.Empty;
  // Links to AspNetUsers.Id (string GUID)
  public string UserId { get; set; } = string.Empty;
  public DateTime ExpiresAt { get; set; }
  // True after this token has been used once
  // Using it again = theft detection
  public bool IsUsed { get; set; }
  // True when revoked by theft detection or manual logout
  public bool IsRevoked { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}