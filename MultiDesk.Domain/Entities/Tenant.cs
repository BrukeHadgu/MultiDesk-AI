namespace MultiDesk.Domain.Entities;

public class Tenant
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Subdomain { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}