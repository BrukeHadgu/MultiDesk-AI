using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MultiDesk.Infrastructure.Identity;
using MultiDesk.Infrastructure.Services;

namespace MultiDesk.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[Tags("Authentication")]
public class AuthController(
    UserManager<MultiDeskUser> userManager,
    RoleManager<IdentityRole> roleManager) : ControllerBase
{
    public record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string Role = "Student");

    public record LoginRequest(
        string Email,
        string Password);

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Register a new user account")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);

        // Generic message prevents account enumeration.
        if (existingUser is not null)
            return Ok(new { message = "Registration request received." });

        var user = new MultiDeskUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TenantId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Identity hashes and stores the password.
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(error => error.Description);
            return BadRequest(new { errors });
        }

        if (!await roleManager.RoleExistsAsync(request.Role))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole(request.Role));

            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(error => error.Description);
                return BadRequest(new { errors });
            }
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, request.Role);

        if (!addRoleResult.Succeeded)
        {
            var errors = addRoleResult.Errors.Select(error => error.Description);
            return BadRequest(new { errors });
        }

        return Ok(new { message = "Registration successful." });
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [EndpointSummary("Login using ASP.NET Core Identity")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Unauthorized(new { detail = "Invalid credentials." });

        if (!user.IsActive)
            return Unauthorized(new { detail = "This account is inactive." });

        if (await userManager.IsLockedOutAsync(user))
        {
            return StatusCode(StatusCodes.Status423Locked, new
            {
                detail = "Account locked due to multiple failed attempts. Try again in 15 minutes."
            });
        }

        var validPassword = await userManager.CheckPasswordAsync(
            user, request.Password);

        if (!validPassword)
        {
            await userManager.AccessFailedAsync(user);
            return Unauthorized(new { detail = "Invalid credentials." });
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new
        {
            userId = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            fullName = user.FullName,
            roles
        });
    }

    [HttpGet("crypto-demo")]
    [AllowAnonymous]
    [EndpointSummary("Demonstrates BCrypt salt uniqueness")]
    public IActionResult CryptoDemo()
    {
        var service = new CryptoDemoService();

        var hash1 = service.HashUserPassword("Password123!");
        var hash2 = service.HashUserPassword("Password123!");

        return Ok(new
        {
            note = "The same password produces different hashes because BCrypt creates a unique salt.",
            hash1,
            hash2,
            hashesMatch = hash1 == hash2,
            bothVerify =
                service.VerifyUserPassword("Password123!", hash1) &&
                service.VerifyUserPassword("Password123!", hash2)
        });
    }
}