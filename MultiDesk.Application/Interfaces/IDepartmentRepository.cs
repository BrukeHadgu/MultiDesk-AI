using MultiDesk.Application.Common;
using MultiDesk.Domain.Entities;

namespace MultiDesk.Application.Interfaces;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<IReadOnlyList<Department>> GetByTenantAsync(
        int tenantId,
        CancellationToken ct = default);

    Task<Department?> GetWithCategoriesAsync(
        int departmentId,
        CancellationToken ct = default);
}