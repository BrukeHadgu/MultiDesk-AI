using MultiDesk.Application.Common;
using MultiDesk.Domain.Entities;
using MultiDesk.Domain.Enums;

namespace MultiDesk.Application.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    // Get all tickets for a tenant with full related data
    Task<IReadOnlyList<Ticket>> GetByTenantAsync(
        int tenantId,
        CancellationToken ct = default);

    // Get tickets by status for a tenant
    Task<IReadOnlyList<Ticket>> GetByStatusAsync(
        int tenantId,
        TicketStatus status,
        CancellationToken ct = default);

    // Get tickets assigned to a specific agent
    Task<IReadOnlyList<Ticket>> GetByAgentAsync(
        int agentId,
        CancellationToken ct = default);

    // Get tickets submitted by a specific student
    Task<IReadOnlyList<Ticket>> GetByStudentAsync(
        int studentId,
        CancellationToken ct = default);

    // Get tickets for a department
    Task<IReadOnlyList<Ticket>> GetByDepartmentAsync(
        int tenantId,
        int departmentId,
        CancellationToken ct = default);

    // Get ticket with all related data (messages, suggestions, agent, student)
    Task<Ticket?> GetWithDetailsAsync(
        int ticketId,
        CancellationToken ct = default);

    // Analytics — average resolution time per department
    Task<Dictionary<string, double>> GetAverageResolutionTimeAsync(
        int tenantId,
        CancellationToken ct = default);

    // Analytics — ticket counts by status
    Task<Dictionary<TicketStatus, int>> GetCountsByStatusAsync(
        int tenantId,
        CancellationToken ct = default);

    // Paginated ticket list
    Task<(IReadOnlyList<Ticket> Items, int TotalCount)> GetPagedAsync(
        int tenantId,
        int page,
        int pageSize,
        TicketStatus? status = null,
        CancellationToken ct = default);
}