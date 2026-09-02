using MultiDesk.Application.DTOs.Users;

namespace MultiDesk.Application.Interfaces;

public interface IUserRepository
{
    Task<UserResponse?> GetByIdAsync(
        string userId, CancellationToken ct = default);

    Task<IReadOnlyList<UserResponse>> GetAllAsync(
        int tenantId, CancellationToken ct = default);

    Task<IReadOnlyList<UserResponse>> GetByRoleAsync(
        int tenantId, string role, CancellationToken ct = default);

    Task<IReadOnlyList<UserResponse>> GetByDepartmentAsync(
        int departmentId, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(
        string email, int tenantId, CancellationToken ct = default);

    Task<UserResponse> UpdateAsync(
        string userId, UpdateUserRequest request,
        int tenantId, CancellationToken ct = default);
}