using MultiDesk.Application.DTOs.Users;
using MultiDesk.Application.Interfaces;
using MultiDesk.Application.Services;

namespace MultiDesk.Infrastructure.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public Task<IReadOnlyList<UserResponse>> GetAllAsync(
        int tenantId, CancellationToken ct = default) =>
        userRepository.GetAllAsync(tenantId, ct);

    public Task<UserResponse?> GetByIdAsync(
        string userId, int tenantId, CancellationToken ct = default) =>
        userRepository.GetByIdAsync(userId, ct);

    public Task<IReadOnlyList<UserResponse>> GetByRoleAsync(
        int tenantId, string role, CancellationToken ct = default) =>
        userRepository.GetByRoleAsync(tenantId, role, ct);

    public Task<UserResponse> UpdateAsync(
        string userId, UpdateUserRequest request,
        int tenantId, CancellationToken ct = default) =>
        userRepository.UpdateAsync(userId, request, tenantId, ct);
}