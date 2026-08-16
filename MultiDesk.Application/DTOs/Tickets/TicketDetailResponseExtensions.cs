namespace MultiDesk.Application.DTOs.Tickets;

public static class TicketDetailResponseExtensions
{
    public static TicketResponse ToTicketResponse(
        this TicketDetailResponse detail) =>
        new(
            detail.Id,
            detail.Title,
            detail.Status,
            detail.Priority,
            detail.DepartmentName,
            detail.CategoryName,
            detail.StudentName,
            detail.AgentName,
            detail.CreatedAt,
            detail.UpdatedAt,
            detail.ResolvedAt,
            detail.Messages.Count);
}