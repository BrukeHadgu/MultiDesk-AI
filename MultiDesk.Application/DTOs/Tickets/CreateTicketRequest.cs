using System.ComponentModel.DataAnnotations;
using MultiDesk.Domain.Enums;

namespace MultiDesk.Application.DTOs.Tickets;

public record CreateTicketRequest
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    [Required]
    [MaxLength(5000)]
    public required string Description { get; init; }

    [Required]
    public int DepartmentId { get; init; }

    [Required]
    public int CategoryId { get; init; }

    public TicketPriority Priority { get; init; } = TicketPriority.Medium;

    // Optional file attachment path — stored locally in V1
    public string? AttachmentPath { get; init; }
}