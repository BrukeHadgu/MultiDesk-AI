namespace MultiDesk.Application.DTOs.Users;

public record UserResponse(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Role,
    bool IsActive,
    string? DepartmentName,
    DateTime CreatedAt);