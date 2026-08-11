namespace MultiDesk.Domain.Entities;

public class Message
{
    public int Id { get; set; }

    public required string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TenantId { get; set; } = 1;

    // Foreign keys
    public int TicketId { get; set; }   // which ticket this message belongs to
    public int SenderId { get; set; }   // who sent it (agent or student)

    // Navigation properties
    public Ticket Ticket { get; set; } = null!;
    public User Sender { get; set; } = null!;
}