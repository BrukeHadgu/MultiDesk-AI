namespace MultiDesk.Domain.Entities;

public class Message
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int TenantId { get; set; } = 1;
    public int TicketId { get; set; }
    public int SenderId { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User Sender { get; set; } = null!;
}