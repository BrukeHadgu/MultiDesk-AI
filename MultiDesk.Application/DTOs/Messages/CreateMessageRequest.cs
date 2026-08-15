using System.ComponentModel.DataAnnotations;

namespace MultiDesk.Application.DTOs.Messages;

public record CreateMessageRequest
{
    [Required]
    [MaxLength(5000)]
    public required string Content { get; init; }
}