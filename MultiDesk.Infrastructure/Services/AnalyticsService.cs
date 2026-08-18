using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.DTOs.Analytics;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Enums;
using MultiDesk.Infrastructure.Persistence;
using MultiDesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace MultiDesk.Infrastructure.Services;

public class AnalyticsService(
    MultiDeskDbContext context,
    UserManager<MultiDeskUser> userManager) : IAnalyticsService
{
    public async Task<DashboardAnalyticsResponse> GetDashboardAsync(
        int tenantId, CancellationToken ct = default)
    {
        // All counts in one query using GroupBy
        var statusCounts = await context.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var total      = statusCounts.Sum(x => x.Count);
        var open       = statusCounts.FirstOrDefault(x => x.Status == TicketStatus.Open)?.Count ?? 0;
        var inProgress = statusCounts.FirstOrDefault(x => x.Status == TicketStatus.InProgress)?.Count ?? 0;
        var resolved   = statusCounts.FirstOrDefault(x => x.Status == TicketStatus.Resolved)?.Count ?? 0;
        var closed     = statusCounts.FirstOrDefault(x => x.Status == TicketStatus.Closed)?.Count ?? 0;

        // Average resolution time in hours
        var avgResolution = await context.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId
                     && t.Status == TicketStatus.Resolved
                     && t.ResolvedAt != null)
            .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            .DefaultIfEmpty(0)
            .AverageAsync(ct);

        // Department breakdown
        var deptBreakdown = await context.Departments
            .AsNoTracking()
            .Where(d => d.TenantId == tenantId && d.IsActive)
            .Select(d => new DepartmentStats(
                d.Name,
                d.Tickets.Count(t => t.Status == TicketStatus.Open),
                d.Tickets.Count(t => t.Status == TicketStatus.Resolved),
                d.Tickets
                    .Where(t => t.Status == TicketStatus.Resolved && t.ResolvedAt != null)
                    .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()))
            .ToListAsync(ct);

        // Agent workloads
        // Replace the agent workload section with this
        var agentRole = "Agent";
        var agentsInRole = await userManager.GetUsersInRoleAsync(agentRole);
        var agentIds = agentsInRole
            .Where(u => u.TenantId == tenantId && u.IsActive)
            .Select(u => u.Id)
            .ToList();

        var agentWorkloads = new List<AgentWorkload>();
        foreach (var agentId in agentIds)
        {
            var agent = agentsInRole.First(u => u.Id == agentId);
            var assigned = await context.Tickets.CountAsync(t =>
                t.AgentId == agentId &&
                (t.Status == TicketStatus.Open ||
                 t.Status == TicketStatus.InProgress), ct);

            var resolvedToday = await context.Tickets.CountAsync(t =>
                t.AgentId == agentId &&
                t.Status == TicketStatus.Resolved &&
                t.ResolvedAt != null &&
                t.ResolvedAt.Value.Date == DateTime.UtcNow.Date, ct);

            agentWorkloads.Add(new AgentWorkload(
                agent.FullName, assigned, resolvedToday));
        }

        // Daily ticket volume (last 7 days)
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).Date;
        var dailyVolume = await context.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId
                     && t.CreatedAt.Date >= sevenDaysAgo)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new DailyTicketVolume(g.Key, g.Count()))
            .OrderBy(d => d.Date)
            .ToListAsync(ct);

        return new DashboardAnalyticsResponse(
            total,
            open,
            inProgress,
            resolved,
            closed,
            Math.Round(avgResolution, 1),
            deptBreakdown,
            agentWorkloads,
            dailyVolume);
    }
}