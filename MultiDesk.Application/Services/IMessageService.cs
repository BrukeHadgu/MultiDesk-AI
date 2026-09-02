using MultiDesk.Application.DTOs.Messages;

namespace MultiDesk.Application.Services;

public interface IMessageService
{
    Task<IReadOnlyList<MessageResponse>> GetByTicketAsync(
        int ticketId, int tenantId,
        CancellationToken ct = default);

    Task<MessageResponse> CreateAsync(
        int ticketId,
        CreateMessageRequest request,
        string senderId,
        string senderRole,
        int tenantId,
        CancellationToken ct = default);
}