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
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int TenantId { get; set; } = 1;
    public int? DepartmentId { get; set; }
    // Refresh token fields
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Department? Department { get; set; }
    public ICollection<Ticket> SubmittedTickets { get; set; } = new List<Ticket>();
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public string FullName => $"{FirstName} {LastName}";
}