using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.Interfaces;
using MultiDesk.Application.DTOs.Users;
using MultiDesk.Infrastructure.Identity;

namespace MultiDesk.Infrastructure.Persistence.Repositories;

public class UserRepository(
    UserManager<MultiDeskUser> userManager) : IUserRepository
{
    public async Task<UserResponse?> GetByIdAsync(
        string userId,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var roles = await userManager.GetRolesAsync(user);
        return MapToResponse(user, roles.FirstOrDefault() ?? "Student");
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(
        int tenantId,
        CancellationToken ct = default)
    {
        var users = await userManager.Users
            .Where(u => u.TenantId == tenantId)
            .ToListAsync(ct);

        var result = new List<UserResponse>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(MapToResponse(user, roles.FirstOrDefault() ?? "Student"));
        }

        return result.OrderBy(u => u.LastName).ToList();
    }

    public async Task<IReadOnlyList<UserResponse>> GetByRoleAsync(
        int tenantId,
        string role,
        CancellationToken ct = default)
    {
        var usersInRole = await userManager.GetUsersInRoleAsync(role);

        var result = usersInRole
            .Where(u => u.TenantId == tenantId)
            .Select(u => MapToResponse(u, role))
            .OrderBy(u => u.LastName)
            .ToList();

        return result;
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        int tenantId,
        CancellationToken ct = default) =>
        await userManager.Users
            .AnyAsync(u => u.Email == email
                        && u.TenantId == tenantId, ct);

    public async Task<UserResponse> UpdateAsync(
        string userId,
        UpdateUserRequest request,
        int tenantId,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
        if (request.DepartmentId.HasValue) user.Department = request.DepartmentId.Value.ToString();

        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        return MapToResponse(user, roles.FirstOrDefault() ?? "Student");
    }

    private static UserResponse MapToResponse(
        MultiDeskUser user, string role) =>
        new(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.FullName,
            role,
            user.IsActive,
            user.Department,
            user.CreatedAt);
}