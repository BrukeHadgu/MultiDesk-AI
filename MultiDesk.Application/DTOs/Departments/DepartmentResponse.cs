namespace MultiDesk.Application.DTOs.Departments;

public record DepartmentResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    int AgentCount,
    int OpenTicketCount);