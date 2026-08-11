using MultiDesk.Domain.Enums;

namespace MultiDesk.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    // Optional file attachment path — stored locally in V1
    // V2 swaps to IStorageProvider (Azure Blob / S3) by changing one DI registration
    public string? AttachmentPath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int TenantId { get; set; } = 1;

    // Foreign keys
    public int StudentId { get; set; }      // who submitted the ticket
    public int? AgentId { get; set; }       // who is handling it (null until assigned)
    public int DepartmentId { get; set; }   // which department handles it
    public int CategoryId { get; set; }     // what type of issue

    // Navigation properties
    public User Student { get; set; } = null!;
    public User? Agent { get; set; }
    public Department Department { get; set; } = null!;
    public Category Category { get; set; } = null!;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<AiSuggestion> AiSuggestions { get; set; } = new List<AiSuggestion>();
}