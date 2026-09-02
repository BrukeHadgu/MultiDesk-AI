namespace MultiDesk.Application.DTOs.Users;

public record UserResponse(
    string Id,           // string GUID — Identity user ID
    string TenantUserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Role,
    bool IsActive,
    int? DepartmentId,
    string? DepartmentName,
    DateTime CreatedAt);