using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.DTOs.Analytics;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Enums;
using MultiDesk.Infrastructure.Identity;
using MultiDesk.Infrastructure.Persistence;

namespace MultiDesk.Infrastructure.Services;

public class AnalyticsService(
    MultiDeskDbContext context,
    UserManager<MultiDeskUser> userManager) : IAnalyticsService
{
    public async Task<DashboardAnalyticsResponse> GetDashboardAsync(
        int tenantId,
        CancellationToken ct = default)
    {
        // Load only the ticket fields needed for analytics.
        // Date subtraction is calculated in C#, after EF has retrieved the data.
        var ticketMetrics = await context.Tickets
            .AsNoTracking()
            .Where(ticket => ticket.TenantId == tenantId)
            .Select(ticket => new
            {
                ticket.DepartmentId,
                ticket.AgentId,
                ticket.Status,
                ticket.CreatedAt,
                ticket.ResolvedAt
            })
            .ToListAsync(ct);

        var total = ticketMetrics.Count;
        var open = ticketMetrics.Count(ticket =>
            ticket.Status == TicketStatus.Open);

        var inProgress = ticketMetrics.Count(ticket =>
            ticket.Status == TicketStatus.InProgress);

        var resolved = ticketMetrics.Count(ticket =>
            ticket.Status == TicketStatus.Resolved);

        var closed = ticketMetrics.Count(ticket =>
            ticket.Status == TicketStatus.Closed);

        // EF Core cannot translate TimeSpan.TotalHours for this PostgreSQL query.
        // Calculate durations safely in memory instead.
        var resolutionHours = ticketMetrics
            .Where(ticket =>
                ticket.Status == TicketStatus.Resolved &&
                ticket.ResolvedAt.HasValue)
            .Select(ticket =>
                (ticket.ResolvedAt!.Value - ticket.CreatedAt).TotalHours)
            .ToList();

        var averageResolutionHours = resolutionHours.Count == 0
            ? 0
            : resolutionHours.Average();

        // Department breakdown
        var departments = await context.Departments
            .AsNoTracking()
            .Where(department =>
                department.TenantId == tenantId &&
                department.IsActive)
            .Select(department => new
            {
                department.Id,
                department.Name
            })
            .OrderBy(department => department.Name)
            .ToListAsync(ct);

        var departmentBreakdown = departments
            .Select(department =>
            {
                var departmentTickets = ticketMetrics
                    .Where(ticket => ticket.DepartmentId == department.Id)
                    .ToList();

                var departmentResolutionHours = departmentTickets
                    .Where(ticket =>
                        ticket.Status == TicketStatus.Resolved &&
                        ticket.ResolvedAt.HasValue)
                    .Select(ticket =>
                        (ticket.ResolvedAt!.Value - ticket.CreatedAt).TotalHours)
                    .ToList();

                var departmentAverageHours =
                    departmentResolutionHours.Count == 0
                        ? 0
                        : departmentResolutionHours.Average();

                return new DepartmentStats(
                    department.Name,
                    departmentTickets.Count(ticket =>
                        ticket.Status == TicketStatus.Open),
                    departmentTickets.Count(ticket =>
                        ticket.Status == TicketStatus.Resolved),
                    Math.Round(departmentAverageHours, 1));
            })
            .ToList();

        // Identity roles are stored separately from MultiDeskUser.
        var agents = await userManager.GetUsersInRoleAsync("Agent");

        var activeAgents = agents
            .Where(agent =>
                agent.TenantId == tenantId &&
                agent.IsActive)
            .ToList();

        var today = DateTime.UtcNow.Date;

        var agentWorkloads = activeAgents
            .Select(agent => new AgentWorkload(
                agent.FullName,
                ticketMetrics.Count(ticket =>
                    ticket.AgentId == agent.Id &&
                    (ticket.Status == TicketStatus.Open ||
                     ticket.Status == TicketStatus.InProgress)),
                ticketMetrics.Count(ticket =>
                    ticket.AgentId == agent.Id &&
                    ticket.Status == TicketStatus.Resolved &&
                    ticket.ResolvedAt.HasValue &&
                    ticket.ResolvedAt.Value.Date == today)))
            .ToList();

        // Daily ticket volume for the last seven days
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).Date;

        var dailyVolume = ticketMetrics
            .Where(ticket => ticket.CreatedAt.Date >= sevenDaysAgo)
            .GroupBy(ticket => ticket.CreatedAt.Date)
            .Select(group => new DailyTicketVolume(
                group.Key,
                group.Count()))
            .OrderBy(item => item.Date)
            .ToList();

        return new DashboardAnalyticsResponse(
            total,
            open,
            inProgress,
            resolved,
            closed,
            Math.Round(averageResolutionHours, 1),
            departmentBreakdown,
            agentWorkloads,
            dailyVolume);
    }
}