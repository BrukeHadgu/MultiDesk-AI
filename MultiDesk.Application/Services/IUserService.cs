using MultiDesk.Application.DTOs.Users;

namespace MultiDesk.Application.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(
        int tenantId, CancellationToken ct = default);

    Task<UserResponse?> GetByIdAsync(
        string userId, int tenantId, CancellationToken ct = default);

    Task<IReadOnlyList<UserResponse>> GetByRoleAsync(
        int tenantId, string role, CancellationToken ct = default);

    Task<UserResponse> UpdateAsync(
        string userId, UpdateUserRequest request,
        int tenantId, CancellationToken ct = default);
}