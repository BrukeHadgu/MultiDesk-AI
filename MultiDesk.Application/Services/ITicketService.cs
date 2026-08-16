using MultiDesk.Application.DTOs.Tickets;
using MultiDesk.Domain.Enums;

namespace MultiDesk.Application.Services;

public interface ITicketService
{
    Task<PagedTicketResponse> GetPagedAsync(
        int tenantId,
        int page,
        int pageSize,
        TicketStatus? status = null,
        CancellationToken ct = default);

    Task<TicketDetailResponse?> GetByIdAsync(
        int ticketId,
        int tenantId,
        CancellationToken ct = default);

    Task<TicketResponse> CreateAsync(
        CreateTicketRequest request,
        int studentId,
        int tenantId,
        CancellationToken ct = default);

    Task<TicketResponse> UpdateAsync(
        int ticketId,
        UpdateTicketRequest request,
        int tenantId,
        CancellationToken ct = default);

    Task DeleteAsync(
        int ticketId,
        int tenantId,
        int deletedBy,
        CancellationToken ct = default);

    Task<TicketResponse> AssignAgentAsync(
        int ticketId,
        int agentId,
        int tenantId,
        CancellationToken ct = default);

    Task<TicketResponse> ChangeStatusAsync(
        int ticketId,
        TicketStatus newStatus,
        int tenantId,
        CancellationToken ct = default);
}