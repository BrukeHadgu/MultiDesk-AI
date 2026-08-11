namespace MultiDesk.Domain.Entities;

public class Category
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int TenantId { get; set; } = 1;

    // Foreign key — category belongs to one department
    public int DepartmentId { get; set; }

    // Navigation properties
    public Department Department { get; set; } = null!;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}