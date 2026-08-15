namespace MultiDesk.Application.DTOs.Departments;

public record CategoryResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    int DepartmentId,
    string DepartmentName);