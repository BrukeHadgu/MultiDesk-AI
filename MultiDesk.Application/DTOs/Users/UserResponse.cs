namespace MultiDesk.Application.DTOs.Users;

public record UserResponse(
    string Id,           // string GUID — Identity user ID
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Role,
    bool IsActive,
    string? DepartmentName,
    DateTime CreatedAt);