using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.DTOs.Departments;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Entities;
using MultiDesk.Infrastructure.Persistence;

namespace MultiDesk.Infrastructure.Services;

public class DepartmentService(MultiDeskDbContext context) : IDepartmentService
{
    public async Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(
        int tenantId, CancellationToken ct = default) =>
        await context.Departments
            .AsNoTracking()
            .Where(d => d.TenantId == tenantId && d.IsActive)
            .Select(d => new DepartmentResponse(
                d.Id,
                d.Name,
                d.Description,
                d.IsActive,
                d.Agents.Count(a => a.IsActive),
                d.Tickets.Count(t => t.Status == Domain.Enums.TicketStatus.Open)))
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

    public async Task<DepartmentResponse?> GetByIdAsync(
        int departmentId, int tenantId,
        CancellationToken ct = default) =>
        await context.Departments
            .AsNoTracking()
            .Where(d => d.Id == departmentId && d.TenantId == tenantId)
            .Select(d => new DepartmentResponse(
                d.Id,
                d.Name,
                d.Description,
                d.IsActive,
                d.Agents.Count(a => a.IsActive),
                d.Tickets.Count(t => t.Status == Domain.Enums.TicketStatus.Open)))
            .FirstOrDefaultAsync(ct);

    public async Task<DepartmentResponse> CreateAsync(
        CreateDepartmentRequest request, int tenantId,
        CancellationToken ct = default)
    {
        var department = new Department
        {
            Name        = request.Name,
            Description = request.Description,
            TenantId    = tenantId,
            IsActive    = true,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };

        context.Departments.Add(department);
        await context.SaveChangesAsync(ct);

        return (await GetByIdAsync(department.Id, tenantId, ct))!;
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(
        int departmentId, int tenantId,
        CancellationToken ct = default) =>
        await context.Categories
            .AsNoTracking()
            .Where(c => c.DepartmentId == departmentId
                     && c.TenantId == tenantId
                     && c.IsActive)
            .Select(c => new CategoryResponse(
                c.Id,
                c.Name,
                c.Description,
                c.IsActive,
                c.DepartmentId,
                c.Department.Name))
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
}