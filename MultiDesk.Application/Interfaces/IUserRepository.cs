using MultiDesk.Application.Common;
using MultiDesk.Domain.Entities;
using MultiDesk.Domain.Enums;

namespace MultiDesk.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken ct = default);

    Task<IReadOnlyList<User>> GetByRoleAsync(
        int tenantId,
        UserRole role,
        CancellationToken ct = default);

    Task<IReadOnlyList<User>> GetAgentsByDepartmentAsync(
        int tenantId,
        int departmentId,
        CancellationToken ct = default);

    Task<bool> EmailExistsAsync(
        string email,
        int tenantId,
        CancellationToken ct = default);
}