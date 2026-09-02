namespace MultiDesk.Application.DTOs.Auth;

public record AuthResponse(
    int UserId,

    string Email,
    string? TenantUserId,
    string FullName,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry);