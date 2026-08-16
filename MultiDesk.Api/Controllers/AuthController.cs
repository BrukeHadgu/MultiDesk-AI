using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MultiDesk.Application.DTOs.Auth;
using MultiDesk.Application.Interfaces;

namespace MultiDesk.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[Tags("Authentication")]
public class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Login with email and password")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var response = await authService.LoginAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Register a new student account")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var response = await authService.RegisterAsync(request, ct: ct);
        return CreatedAtAction(nameof(Login), response);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Refresh access token using refresh token")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var response = await authService.RefreshTokenAsync(request, ct);
        return Ok(response);
    }
}