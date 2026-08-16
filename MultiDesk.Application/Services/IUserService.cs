using MultiDesk.Application.DTOs.Users;
using MultiDesk.Domain.Enums;

namespace MultiDesk.Application.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(
        int tenantId,
        CancellationToken ct = default);

    Task<UserResponse?> GetByIdAsync(
        int userId,
        int tenantId,
        CancellationToken ct = default);

    Task<IReadOnlyList<UserResponse>> GetByRoleAsync(
        int tenantId,
        UserRole role,
        CancellationToken ct = default);

    Task<UserResponse> UpdateAsync(
        int userId,
        UpdateUserRequest request,
        int tenantId,
        CancellationToken ct = default);
}