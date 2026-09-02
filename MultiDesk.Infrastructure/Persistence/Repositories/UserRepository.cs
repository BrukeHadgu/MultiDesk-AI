using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.DTOs.Users;
using MultiDesk.Application.Interfaces;
using MultiDesk.Infrastructure.Identity;
using MultiDesk.Infrastructure.Persistence;

namespace MultiDesk.Infrastructure.Persistence.Repositories;

public class UserRepository(
    UserManager<MultiDeskUser> userManager,
    MultiDeskDbContext context) : IUserRepository
{
    public async Task<UserResponse?> GetByIdAsync(
        string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var roles = await userManager.GetRolesAsync(user);
        var department = user.DepartmentId.HasValue
            ? await context.Departments.FindAsync(user.DepartmentId.Value, ct)
            : null;

        return MapToResponse(user, roles.FirstOrDefault() ?? "Student", department?.Name);
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(
        int tenantId, CancellationToken ct = default)
    {
        var users = await userManager.Users
            .Where(u => u.TenantId == tenantId)
            .ToListAsync(ct);

        var result = new List<UserResponse>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var department = user.DepartmentId.HasValue
                ? await context.Departments.FindAsync(user.DepartmentId.Value, ct)
                : null;
            result.Add(MapToResponse(user, roles.FirstOrDefault() ?? "Student", department?.Name));
        }

        return result.OrderBy(u => u.LastName).ToList();
    }

    public async Task<IReadOnlyList<UserResponse>> GetByRoleAsync(
        int tenantId, string role, CancellationToken ct = default)
    {
        var usersInRole = await userManager.GetUsersInRoleAsync(role);
        var result = new List<UserResponse>();

        foreach (var user in usersInRole.Where(u => u.TenantId == tenantId))
        {
            var department = user.DepartmentId.HasValue
                ? await context.Departments.FindAsync(user.DepartmentId.Value, ct)
                : null;
            result.Add(MapToResponse(user, role, department?.Name));
        }

        return result.OrderBy(u => u.LastName).ToList();
    }

    public async Task<IReadOnlyList<UserResponse>> GetByDepartmentAsync(
        int departmentId, CancellationToken ct = default)
    {
        var agents = await userManager.Users
            .Where(u => u.DepartmentId == departmentId && u.IsActive)
            .ToListAsync(ct);

        var department = await context.Departments.FindAsync(departmentId, ct);
        var result = new List<UserResponse>();

        foreach (var user in agents)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(MapToResponse(user, roles.FirstOrDefault() ?? "Agent", department?.Name));
        }

        return result;
    }

    public async Task<bool> EmailExistsAsync(
        string email, int tenantId, CancellationToken ct = default) =>
        await userManager.Users
            .AnyAsync(u => u.Email == email && u.TenantId == tenantId, ct);

    public async Task<UserResponse> UpdateAsync(
        string userId, UpdateUserRequest request,
        int tenantId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
        if (request.DepartmentId.HasValue) user.DepartmentId = request.DepartmentId.Value;

        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        return (await GetByIdAsync(userId, ct))!;
    }

    private static UserResponse MapToResponse(
        MultiDeskUser user, string role, string? departmentName) =>
        new(
            user.Id,
            user.TenantUserId,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.FullName,
            role,
            user.IsActive,
            user.DepartmentId,
            departmentName,
            user.CreatedAt);
}