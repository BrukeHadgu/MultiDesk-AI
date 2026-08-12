namespace MultiDesk.Domain.Entities;
public class AiSuggestion
{
    public int Id { get; set; }
    public required string SuggestedText { get; set; }
    public bool Accepted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public int TenantId { get; set; } = 1;
    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;
}