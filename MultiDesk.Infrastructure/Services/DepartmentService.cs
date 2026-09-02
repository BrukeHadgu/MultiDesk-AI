using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.DTOs.Departments;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Entities;
using MultiDesk.Infrastructure.Identity;
using MultiDesk.Infrastructure.Persistence;
using MultiDesk.Domain.Enums;

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
                                 && a.DepartmentId == d.Id
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
        var agentCount = agentsInRole.Count
        (a => a.TenantId == tenantId && a.DepartmentId == d.Id && a.IsActive);

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
    int departmentId,
    int tenantId,
    CancellationToken ct = default)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .Include(category => category.Department)
            .Where(category =>
                category.DepartmentId == departmentId &&
                category.TenantId == tenantId &&
                category.IsActive)
            .OrderBy(category => category.Name)
            .ToListAsync(ct);

        return categories
            .Select(category => new CategoryResponse(
                category.Id,
                category.Name,
                category.Description,
                category.IsActive,
                category.DepartmentId,
                category.Department.Name,
                category.DefaultPriority.ToString()))
            .ToList();
    }

    public async Task<CategoryResponse> CreateCategoryAsync(
        int departmentId, CreateCategoryRequest request,
        int tenantId, CancellationToken ct = default)
    {
        var dept = await context.Departments
            .FirstOrDefaultAsync(d => d.Id == departmentId
                                   && d.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException(
                $"Department {departmentId} not found.");

        // Parse the priority string to enum
        if (!Enum.TryParse<TicketPriority>(
            request.DefaultPriority, out var priority))
            priority = TicketPriority.Medium;

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            DepartmentId = departmentId,
            TenantId = tenantId,
            IsActive = true,
            DefaultPriority = priority,   // ADD
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync(ct);

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            departmentId,
            dept.Name,
            category.DefaultPriority.ToString());  // ADD
    }
}