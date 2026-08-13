using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.Interfaces;
using MultiDesk.Domain.Entities;

namespace MultiDesk.Infrastructure.Persistence.Repositories;

public class DepartmentRepository(MultiDeskDbContext context)
    : Repository<Department>(context), IDepartmentRepository
{
    public async Task<IReadOnlyList<Department>> GetByTenantAsync(
        int tenantId, CancellationToken ct = default) =>
        await Context.Departments
            .AsNoTracking()
            .Where(d => d.TenantId == tenantId && d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

    public async Task<Department?> GetWithCategoriesAsync(
        int departmentId, CancellationToken ct = default) =>
        await Context.Departments
            .AsNoTracking()
            .Where(d => d.Id == departmentId)
            .Include(d => d.Categories.Where(c => c.IsActive))
            .FirstOrDefaultAsync(ct);
}