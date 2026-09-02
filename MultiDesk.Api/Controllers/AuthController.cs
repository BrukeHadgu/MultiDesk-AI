using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiDesk.Domain.Entities;
using MultiDesk.Infrastructure.Identity;
using MultiDesk.Infrastructure.Persistence;
using MultiDesk.Infrastructure.Services;
using MultiDesk.Application.Services;

namespace MultiDesk.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[Tags("Authentication")]
public class AuthController(
    UserManager<MultiDeskUser> userManager,
    RoleManager<IdentityRole> roleManager,
    MultiDeskDbContext context,
    TokenService tokenService) : ControllerBase
{
    // Register a new user account

    public record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string Role = "Student");

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("Register")]
    public async Task<IActionResult> Register(
    [FromBody] RegisterRequest request,
    [FromServices] ITenantService tenantService,
    [FromServices] TenantUserIdGenerator idGenerator)
    {
        // Resolve tenant from email domain
        var tenantId = await tenantService.ResolveTenantFromEmailAsync(
            request.Email);

        if (tenantId is null)
            return BadRequest(new
            {
                errors = new[]
                {
                "Please use your university email address. " +
                "Personal email domains are not accepted."
            }
            });

        // Prevent account enumeration
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Ok(new { message = "Registration request received." });

        // Generate tenant-scoped ID
        var tenantUserId = await idGenerator.GenerateAsync(
            "Student", tenantId.Value);

        var user = new MultiDeskUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TenantId = tenantId.Value,    // from email domain
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TenantUserId = tenantUserId
        };

        // Public registration ALWAYS creates Students — never Admin/Agent
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new
            {
                errors = result.Errors.Select(e => e.Description)
            });

        if (!await roleManager.RoleExistsAsync("Student"))
            await roleManager.CreateAsync(new IdentityRole("Student"));

        await userManager.AddToRoleAsync(user, "Student");

        return Ok(new
        {
            message = "Registration successful.",
            tenantUserId,
            university = user.TenantId
        });
    }

    // Login and issue JWT + refresh token

    public record LoginRequest(string Email, string Password);

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [EndpointSummary("Login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Unauthorized(new { detail = "Invalid credentials." });
        if (await userManager.IsLockedOutAsync(user))
        {
            return StatusCode(423, new
            {
                detail = "Account locked due to multiple failed login attempts. Try again in 15 minutes."
            });
        }
        var validPassword = await userManager
            .CheckPasswordAsync(user, request.Password);
        if (!validPassword)
        {
            await userManager.AccessFailedAsync(user);
            return Unauthorized(new { detail = "Invalid credentials." });
        }
        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.GenerateJwt(user, roles);
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        return Ok(new
        {
            userId = user.Id,
            tenantUserId = user.TenantUserId,
            email = user.Email,
            fullName = user.FullName,
            role = roles.FirstOrDefault(),
            tenantId = user.TenantId,
            accessToken,
            refreshToken = refreshToken.Token,
            accessTokenExpiry = tokenService.GetExpiry(),
            mustChangePassword = user.MustChangePassword
        });
    }
    // Refresh access token using a refresh token (single-use, rotates on every call)

    public record RefreshRequest(string RefreshToken);

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Refresh access token")]
    [EndpointDescription(
        "Single-use tokens. Reusing an old token triggers theft detection " +
        "and revokes ALL active sessions for that user.")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request)
    {
        var storedToken = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken is null)
            return Unauthorized(new { detail = "Invalid refresh token." });

        // Theft detection: if a USED token is submitted, someone is replaying a stolen token
        // If a USED token is submitted, someone is replaying a stolen token
        // Revoke ALL sessions for this user immediately
        if (storedToken.IsUsed)
        {
            var allUserTokens = await context.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId)
                .ToListAsync();

            foreach (var t in allUserTokens)
                t.IsRevoked = true;

            await context.SaveChangesAsync();

            return Unauthorized(new
            {
                detail = "Token theft detected. All user sessions have been revoked."
            });
        }

        // Reject if the token has been revoked or expired
        if (storedToken.IsRevoked)
            return Unauthorized(new { detail = "Refresh token has been revoked." });

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            return Unauthorized(new { detail = "Refresh token has expired." });

        // Rotate the refresh token: mark the old one as used and issue a new one
        storedToken.IsUsed = true;

        // Issue a new refresh token for the user
        var newRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = storedToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync();

        var user = await userManager.FindByIdAsync(storedToken.UserId);
        var roles = await userManager.GetRolesAsync(user!);
        var newJwt = tokenService.GenerateJwt(user!, roles);

        return Ok(new
        {
            accessToken = newJwt,
            refreshToken = newRefreshToken.Token,
            accessTokenExpiry = tokenService.GetExpiry()
        });
    }

    // crypto-demo endpoint to demonstrate BCrypt salt uniqueness

    [HttpGet("crypto-demo")]
    [AllowAnonymous]
    [EndpointSummary("Demonstrates salt uniqueness in password hashing")]
    public IActionResult CryptoDemo()
    {
        var service = new CryptoDemoService();
        var hash1 = service.HashUserPassword("Password123!");
        var hash2 = service.HashUserPassword("Password123!");

        return Ok(new
        {
            note = "Same password produces different hashes because of unique random salts",
            hash1,
            hash2,
            hashesMatch = hash1 == hash2,
            bothVerify = service.VerifyUserPassword("Password123!", hash1)
                       && service.VerifyUserPassword("Password123!", hash2)
        });
    }

    // get current user info from JWT token

    [HttpGet("me")]
    [Authorize]
    [EndpointSummary("Get current user info from JWT token")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userManager.FindByIdAsync(userId!);
        if (user is null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new
        {
            userId = user.Id,
            email = user.Email,
            fullName = user.FullName,
            role = roles.FirstOrDefault(),
            tenantId = user.TenantId,
            isActive = user.IsActive
        });
    }

    public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("Change password — required on first login for admin and agent")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Unauthorized();

        var result = await userManager.ChangePasswordAsync(
            user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(new
            {
                errors = result.Errors.Select(e => e.Description)
            });

        // Clear the force change flag
        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        return Ok(new { message = "Password changed successfully." });
    }
}