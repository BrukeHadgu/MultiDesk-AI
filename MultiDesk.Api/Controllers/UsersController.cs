using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiDesk.Api.Extensions;
using MultiDesk.Application.DTOs.Users;
using MultiDesk.Application.Services;

namespace MultiDesk.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
[Tags("Users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    [EndpointSummary("Get all users (Admin only)")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? role,
        CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var users = role is not null
            ? await userService.GetByRoleAsync(tenantId, role, ct)
            : await userService.GetAllAsync(tenantId, ct);
        return Ok(users);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get user by ID (Admin only)")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var user = await userService.GetByIdAsync(id, tenantId, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [EndpointSummary("Update user details (Admin only)")]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var user = await userService.UpdateAsync(id, request, tenantId, ct);
        return Ok(user);
    }
}