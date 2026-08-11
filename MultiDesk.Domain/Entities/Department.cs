namespace MultiDesk.Domain.Entities;

public class Department
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TenantId { get; set; } = 1;

    // Navigation properties
    public ICollection<Category> Categories { get; set; } = new List<Category>();

    public ICollection<User> Agents { get; set; } = new List<User>();

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}