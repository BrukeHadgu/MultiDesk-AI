namespace MultiDesk.Domain.Entities;

public class AiSuggestion
{
    public int Id { get; set; }

    public required string SuggestedText { get; set; }

    // Tracks whether the agent used this suggestion
    // Future analytics: AI adoption rate — how often agents accept vs ignore suggestions
    public bool Accepted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TenantId { get; set; } = 1;

    // Foreign key — suggestion belongs to one ticket
    public int TicketId { get; set; }

    // Navigation property
    public Ticket Ticket { get; set; } = null!;
}