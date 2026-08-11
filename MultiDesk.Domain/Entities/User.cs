using MultiDesk.Domain.Enums;

namespace MultiDesk.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Multi-tenancy ready — TenantId is here from day one
    // V1 uses a single tenant, V2 activates full isolation
    public int TenantId { get; set; } = 1;

    // Agent belongs to one department (null for Admin and Student)
    public int? DepartmentId { get; set; }

    // Navigation properties
    public Department? Department { get; set; }

    public ICollection<Ticket> SubmittedTickets { get; set; } = new List<Ticket>();

    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    public ICollection<Message> Messages { get; set; } = new List<Message>();

    // Computed property — not stored in DB
    public string FullName => $"{FirstName} {LastName}";
}