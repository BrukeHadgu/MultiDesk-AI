using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MultiDesk.Application.DTOs.Auth;
using MultiDesk.Application.Interfaces;
using MultiDesk.Domain.Entities;
using MultiDesk.Domain.Enums;
using MultiDesk.Infrastructure.Persistence;

namespace MultiDesk.Infrastructure.Services;

public class AuthService(
    MultiDeskDbContext context,
    IJwtService jwtService,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly int _refreshTokenExpiryDays =
        int.Parse(configuration["JwtSettings:RefreshTokenExpiryDays"]!);

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default)
    {
        // Find user by email — IgnoreQueryFilters to allow deleted check
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.IsDeleted)
            throw new UnauthorizedAccessException("Account has been deactivated.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is not active.");

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await GenerateAndSaveTokensAsync(user, ct);
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        int tenantId = 1,
        CancellationToken ct = default)
    {
        // Check email is unique within tenant
        var emailExists = await context.Users
            .AnyAsync(u => u.Email == request.Email
                        && u.TenantId == tenantId, ct);

        if (emailExists)
            throw new InvalidOperationException(
                $"Email '{request.Email}' is already registered.");

        var user = new User
        {
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName    = request.FirstName,
            LastName     = request.LastName,
            Role         = UserRole.Student,  // default role on self-register
            IsActive     = true,
            TenantId     = tenantId,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "New student registered: {Email} (TenantId: {TenantId})",
            user.Email, user.TenantId);

        return await GenerateAndSaveTokensAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken ct = default)
    {
        // Find user by refresh token
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        // Check token hasn't expired
        if (user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired. Please log in again.");

        return await GenerateAndSaveTokensAsync(user, ct);
    }

    // ── Private Helpers ──────────────────────────────────────────────

    private async Task<AuthResponse> GenerateAndSaveTokensAsync(
        User user,
        CancellationToken ct)
    {
        var accessToken   = jwtService.GenerateAccessToken(user);
        var refreshToken  = jwtService.GenerateRefreshToken();
        var accessExpiry  = jwtService.GetAccessTokenExpiry();

        // Save refresh token to database
        user.RefreshToken       = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);
        user.UpdatedAt          = DateTime.UtcNow;

        context.Users.Update(user);
        await context.SaveChangesAsync(ct);

        return new AuthResponse(
            UserId:            user.Id,
            Email:             user.Email,
            FullName:          user.FullName,
            Role:              user.Role.ToString(),
            AccessToken:       accessToken,
            RefreshToken:      refreshToken,
            AccessTokenExpiry: accessExpiry);
    }
}