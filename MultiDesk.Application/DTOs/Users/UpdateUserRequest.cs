using System.ComponentModel.DataAnnotations;

namespace MultiDesk.Application.DTOs.Users;

public record UpdateUserRequest
{
    [MaxLength(100)]
    public string? FirstName { get; init; }

    [MaxLength(100)]
    public string? LastName { get; init; }

    public bool? IsActive { get; init; }

    // Admin only — assign agent to department
    public int? DepartmentId { get; init; }
}