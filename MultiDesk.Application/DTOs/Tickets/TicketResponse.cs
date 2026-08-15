namespace MultiDesk.Application.DTOs.Tickets;

// Used in list views — no messages, no AI suggestions
public record TicketResponse(
    int Id,
    string Title,
    string Status,
    string Priority,
    string DepartmentName,
    string CategoryName,
    string StudentName,
    string? AgentName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ResolvedAt,
    int MessageCount);