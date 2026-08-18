using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MultiDesk.Infrastructure.Identity;

namespace MultiDesk.Infrastructure.Services;

public class TokenService(IConfiguration config)
{
  public string GenerateJwt(MultiDeskUser user, IList<string> roles)
  {
    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email,          user.Email ?? string.Empty),
            new Claim("FirstName",               user.FirstName),
            new Claim("LastName",                user.LastName),
            new Claim("tenantId",                user.TenantId.ToString())
        };

    // Add all roles as separate claims
    foreach (var role in roles)
      claims.Add(new Claim(ClaimTypes.Role, role));

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
    var creds = new SigningCredentials(
        key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: config["Jwt:Issuer"],
        audience: config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(
            int.Parse(config["Jwt:ExpiryMinutes"]!)),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public DateTime GetExpiry() =>
      DateTime.UtcNow.AddMinutes(
          int.Parse(config["Jwt:ExpiryMinutes"]!));
}