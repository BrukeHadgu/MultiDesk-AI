using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.DTOs.Users;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Enums;
using MultiDesk.Infrastructure.Persistence;

namespace MultiDesk.Infrastructure.Services;

public class UserService(MultiDeskDbContext context) : IUserService
{
    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(
        int tenantId, CancellationToken ct = default) =>
        await context.Users
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId)
            .Select(u => new UserResponse(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.FirstName + " " + u.LastName,
                u.Role.ToString(),
                u.IsActive,
                u.Department != null ? u.Department.Name : null,
                u.CreatedAt))
            .OrderBy(u => u.LastName)
            .ToListAsync(ct);

    public async Task<UserResponse?> GetByIdAsync(
        int userId, int tenantId,
        CancellationToken ct = default) =>
        await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && u.TenantId == tenantId)
            .Select(u => new UserResponse(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.FirstName + " " + u.LastName,
                u.Role.ToString(),
                u.IsActive,
                u.Department != null ? u.Department.Name : null,
                u.CreatedAt))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<UserResponse>> GetByRoleAsync(
        int tenantId, UserRole role,
        CancellationToken ct = default) =>
        await context.Users
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.Role == role)
            .Select(u => new UserResponse(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.FirstName + " " + u.LastName,
                u.Role.ToString(),
                u.IsActive,
                u.Department != null ? u.Department.Name : null,
                u.CreatedAt))
            .OrderBy(u => u.LastName)
            .ToListAsync(ct);

    public async Task<UserResponse> UpdateAsync(
        int userId, UpdateUserRequest request, int tenantId,
        CancellationToken ct = default)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId
                                   && u.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        if (request.FirstName is not null) user.FirstName    = request.FirstName;
        if (request.LastName  is not null) user.LastName     = request.LastName;
        if (request.IsActive.HasValue)     user.IsActive     = request.IsActive.Value;
        if (request.DepartmentId.HasValue) user.DepartmentId = request.DepartmentId.Value;

        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        return (await GetByIdAsync(userId, tenantId, ct))!;
    }
}