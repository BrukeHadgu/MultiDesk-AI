using System.ComponentModel.DataAnnotations;
using MultiDesk.Domain.Enums;

namespace MultiDesk.Application.DTOs.Tickets;

public record UpdateTicketRequest
{
    [MaxLength(200)]
    public string? Title { get; init; }

    [MaxLength(5000)]
    public string? Description { get; init; }

    public TicketStatus? Status { get; init; }

    public TicketPriority? Priority { get; init; }

    // Agent ID is now string (Identity GUID)
    public string? AgentId { get; init; }
}