using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.DTOs.Messages;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Entities;
using MultiDesk.Infrastructure.Identity;
using MultiDesk.Infrastructure.Persistence;

namespace MultiDesk.Infrastructure.Services;

public class MessageService(
    MultiDeskDbContext context,
    UserManager<MultiDeskUser> userManager) : IMessageService
{
    public async Task<IReadOnlyList<MessageResponse>> GetByTicketAsync(
        int ticketId, int tenantId,
        CancellationToken ct = default)
    {
        var messages = await context.Messages
            .AsNoTracking()
            .Where(m => m.TicketId == ticketId && m.TenantId == tenantId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

        // Load senders separately
        var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
        var senders = await userManager.Users
            .Where(u => senderIds.Contains(u.Id))
            .ToListAsync(ct);
        var senderMap = senders.ToDictionary(u => u.Id);

        var result = new List<MessageResponse>();
        foreach (var m in messages)
        {
            senderMap.TryGetValue(m.SenderId, out var sender);
            var roles = sender != null
                ? await userManager.GetRolesAsync(sender)
                : new List<string>();

            result.Add(new MessageResponse(
                m.Id,
                m.Content,
                m.SenderId,
                sender?.FullName ?? "Unknown",
                roles.FirstOrDefault() ?? "Student",
                m.CreatedAt));
        }

        return result;
    }

    public async Task<MessageResponse> CreateAsync(
        int ticketId,
        CreateMessageRequest request,
        string senderId,
        string senderRole,
        int tenantId,
        CancellationToken ct = default)
    {
        var ticket = await context.Tickets
        .FirstOrDefaultAsync(t => t.Id == ticketId
                               && t.TenantId == tenantId, ct)
        ?? throw new KeyNotFoundException($"Ticket {ticketId} not found.");

        // Students can only message their own tickets
        if (senderRole == "Student" && ticket.StudentId != senderId)
            throw new UnauthorizedAccessException(
                "You can only add messages to your own tickets.");

        var message = new Message
        {
            Content = request.Content,
            TicketId = ticketId,
            SenderId = senderId,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Messages.Add(message);

        // Update ticket UpdatedAt
        if (ticket is not null)
            ticket.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        var sender = await userManager.FindByIdAsync(senderId);
        var roles = sender != null
            ? await userManager.GetRolesAsync(sender)
            : new List<string>();

        return new MessageResponse(
            message.Id,
            message.Content,
            message.SenderId,
            sender?.FullName ?? "Unknown",
            roles.FirstOrDefault() ?? "Student",
            message.CreatedAt);
    }
}