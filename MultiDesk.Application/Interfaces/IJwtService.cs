using MultiDesk.Domain.Entities;

namespace MultiDesk.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiry();
}