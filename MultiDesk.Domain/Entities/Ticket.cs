using MultiDesk.Domain.Enums;

namespace MultiDesk.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public string? AttachmentPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public int TenantId { get; set; } = 1;
    public int DepartmentId { get; set; }
    public int CategoryId { get; set; }

    // Identity user IDs — strings, no navigation properties
    public string StudentId { get; set; } = string.Empty;
    public string? AgentId { get; set; }

    // These navigation properties remain — they point to Domain entities
    public Tenant Tenant { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<AiSuggestion> AiSuggestions { get; set; } = new List<AiSuggestion>();
}