namespace MultiDesk.Infrastructure.Services;

public class CryptoDemoService
{
  public string HashUserPassword(string plainText)
  {
    // BCrypt automatically generates a unique salt
    // workFactor 12 = 2^12 key expansion iterations
    // Higher = slower = harder to brute force
    return BCrypt.Net.BCrypt.HashPassword(plainText, workFactor: 12);
  }

  public bool VerifyUserPassword(string plainText, string hashedDbPassword)
  {
    return BCrypt.Net.BCrypt.Verify(plainText, hashedDbPassword);
  }
}