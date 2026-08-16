using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.DTOs.Messages;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Entities;
using MultiDesk.Infrastructure.Persistence;

namespace MultiDesk.Infrastructure.Services;

public class MessageService(MultiDeskDbContext context) : IMessageService
{
    public async Task<IReadOnlyList<MessageResponse>> GetByTicketAsync(
        int ticketId, int tenantId,
        CancellationToken ct = default) =>
        await context.Messages
            .AsNoTracking()
            .Where(m => m.TicketId == ticketId && m.TenantId == tenantId)
            .Include(m => m.Sender)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageResponse(
                m.Id,
                m.Content,
                m.SenderId,
                m.Sender.FirstName + " " + m.Sender.LastName,
                m.Sender.Role.ToString(),
                m.CreatedAt))
            .ToListAsync(ct);

    public async Task<MessageResponse> CreateAsync(
        int ticketId,
        CreateMessageRequest request,
        int senderId,
        int tenantId,
        CancellationToken ct = default)
    {
        // Verify ticket exists and belongs to tenant
        var ticketExists = await context.Tickets
            .AnyAsync(t => t.Id == ticketId && t.TenantId == tenantId, ct);

        if (!ticketExists)
            throw new KeyNotFoundException($"Ticket {ticketId} not found.");

        var message = new Message
        {
            Content   = request.Content,
            TicketId  = ticketId,
            SenderId  = senderId,
            TenantId  = tenantId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Messages.Add(message);

        // Update ticket UpdatedAt when new message arrives
        var ticket = await context.Tickets.FindAsync(ticketId, ct);
        if (ticket is not null)
            ticket.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        var sender = await context.Users.FindAsync(senderId, ct);

        return new MessageResponse(
            message.Id,
            message.Content,
            message.SenderId,
            sender!.FullName,
            sender.Role.ToString(),
            message.CreatedAt);
    }
}