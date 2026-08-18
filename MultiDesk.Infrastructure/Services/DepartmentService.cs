using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.DTOs.Departments;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Entities;
using MultiDesk.Infrastructure.Identity;
using MultiDesk.Infrastructure.Persistence;

namespace MultiDesk.Infrastructure.Services;

public class DepartmentService(
    MultiDeskDbContext context,
    UserManager<MultiDeskUser> userManager) : IDepartmentService
{
    public async Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(
        int tenantId, CancellationToken ct = default)
    {
        var departments = await context.Departments
            .AsNoTracking()
            .Where(d => d.TenantId == tenantId && d.IsActive)
            .Include(d => d.Tickets)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

        // Count agents per department from Identity users
        var agentsInRole = await userManager.GetUsersInRoleAsync("Agent");

        return departments.Select(d => new DepartmentResponse(
            d.Id,
            d.Name,
            d.Description,
            d.IsActive,
            agentsInRole.Count(a => a.TenantId == tenantId
                                 && a.Department == d.Name
                                 && a.IsActive),
            d.Tickets.Count(t => t.Status == Domain.Enums.TicketStatus.Open)))
        .ToList();
    }

    public async Task<DepartmentResponse?> GetByIdAsync(
        int departmentId, int tenantId,
        CancellationToken ct = default)
    {
        var d = await context.Departments
            .AsNoTracking()
            .Where(d => d.Id == departmentId && d.TenantId == tenantId)
            .Include(d => d.Tickets)
            .FirstOrDefaultAsync(ct);

        if (d is null) return null;

        var agentsInRole = await userManager.GetUsersInRoleAsync("Agent");
        var agentCount = agentsInRole.Count(a => a.TenantId == tenantId
                                                 && a.Department == d.Name
                                                 && a.IsActive);

        return new DepartmentResponse(
            d.Id,
            d.Name,
            d.Description,
            d.IsActive,
            agentCount,
            d.Tickets.Count(t => t.Status == Domain.Enums.TicketStatus.Open));
    }

    public async Task<DepartmentResponse> CreateAsync(
        CreateDepartmentRequest request, int tenantId,
        CancellationToken ct = default)
    {
        var department = new Department
        {
            Name = request.Name,
            Description = request.Description,
            TenantId = tenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
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