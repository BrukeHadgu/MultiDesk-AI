namespace MultiDesk.Domain.Entities;

public class Tenant
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Subdomain { get; set; }
    public bool IsActive { get; set; } = true;

    // Email domain used to auto assign students to this tenant
    //"cotbe.edu", "aau.edu", "unity.edu"
    public required string EmailDomain { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}