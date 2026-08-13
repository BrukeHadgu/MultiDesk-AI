using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.Interfaces;
using MultiDesk.Domain.Entities;
using MultiDesk.Domain.Enums;

namespace MultiDesk.Infrastructure.Persistence.Repositories;

public class UserRepository(MultiDeskDbContext context)
    : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(
        string email, CancellationToken ct = default) =>
        await Context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<IReadOnlyList<User>> GetByRoleAsync(
        int tenantId, UserRole role, CancellationToken ct = default) =>
        await Context.Users
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.Role == role)
            .OrderBy(u => u.LastName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<User>> GetAgentsByDepartmentAsync(
        int tenantId, int departmentId, CancellationToken ct = default) =>
        await Context.Users
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId
                     && u.Role == UserRole.Agent
                     && u.DepartmentId == departmentId
                     && u.IsActive)
            .OrderBy(u => u.LastName)
            .ToListAsync(ct);

    public async Task<bool> EmailExistsAsync(
        string email, int tenantId, CancellationToken ct = default) =>
        await Context.Users
            .AnyAsync(u => u.Email == email && u.TenantId == tenantId, ct);
}