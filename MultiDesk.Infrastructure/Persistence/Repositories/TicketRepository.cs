using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.Interfaces;
using MultiDesk.Domain.Entities;
using MultiDesk.Domain.Enums;

namespace MultiDesk.Infrastructure.Persistence.Repositories;

public class TicketRepository(MultiDeskDbContext context)
    : Repository<Ticket>(context), ITicketRepository
{
    public async Task<IReadOnlyList<Ticket>> GetByTenantAsync(
        int tenantId, CancellationToken ct = default) =>
        await Context.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .Include(t => t.Department)
            .Include(t => t.Category)
            .Include(t => t.Messages)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Ticket>> GetByStatusAsync(
        int tenantId, TicketStatus status,
        CancellationToken ct = default) =>
        await Context.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.Status == status)
            .Include(t => t.Department)
            .Include(t => t.Category)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Ticket>> GetByAgentAsync(
        string agentId, CancellationToken ct = default) =>
        await Context.Tickets
            .AsNoTracking()
            .Where(t => t.AgentId == agentId)
            .Include(t => t.Department)
            .Include(t => t.Category)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Ticket>> GetByStudentAsync(
        string studentId, CancellationToken ct = default) =>
        await Context.Tickets
            .AsNoTracking()
            .Where(t => t.StudentId == studentId)
            .Include(t => t.Department)
            .Include(t => t.Category)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Ticket>> GetByDepartmentAsync(
        int tenantId, int departmentId,
        CancellationToken ct = default) =>
        await Context.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId
                     && t.DepartmentId == departmentId)
            .Include(t => t.Department)
            .Include(t => t.Category)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<Ticket?> GetWithDetailsAsync(
        int ticketId, CancellationToken ct = default) =>
        await Context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId)
            .Include(t => t.Department)
            .Include(t => t.Category)
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .Include(t => t.AiSuggestions)
            .FirstOrDefaultAsync(ct);

    public async Task<Dictionary<string, double>> GetAverageResolutionTimeAsync(
        int tenantId, CancellationToken ct = default)
    {
        var result = await Context.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId
                     && t.Status == TicketStatus.Resolved
                     && t.ResolvedAt != null)
            .GroupBy(t => t.Department.Name)
            .Select(g => new
            {
                Department = g.Key,
                AvgHours = g.Average(t =>
                    (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            })
            .ToListAsync(ct);

        return result.ToDictionary(x => x.Department, x => x.AvgHours);
    }

    public async Task<Dictionary<TicketStatus, int>> GetCountsByStatusAsync(
        int tenantId, CancellationToken ct = default)
    {
        var result = await Context.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return result.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<(IReadOnlyList<Ticket> Items, int TotalCount)> GetPagedAsync(
        int tenantId, int page, int pageSize,
        TicketStatus? status = null,
        CancellationToken ct = default)
    {
        var query = Context.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(t => t.Department)
            .Include(t => t.Category)
            .Include(t => t.Messages)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}