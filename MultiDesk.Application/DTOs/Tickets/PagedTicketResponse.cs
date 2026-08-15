namespace MultiDesk.Application.DTOs.Tickets;

public record PagedTicketResponse(
    IReadOnlyList<TicketResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNext,
    bool HasPrevious);