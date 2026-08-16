namespace MultiDesk.Application.DTOs.Analytics;

public record DashboardAnalyticsResponse(
    int TotalTickets,
    int OpenTickets,
    int InProgressTickets,
    int ResolvedTickets,
    int ClosedTickets,
    double AverageResolutionHours,
    IReadOnlyList<DepartmentStats> DepartmentBreakdown,
    IReadOnlyList<AgentWorkload> AgentWorkloads,
    IReadOnlyList<DailyTicketVolume> DailyVolume);

public record DepartmentStats(
    string DepartmentName,
    int OpenCount,
    int ResolvedCount,
    double AvgResolutionHours);

public record AgentWorkload(
    string AgentName,
    int AssignedTickets,
    int ResolvedToday);

public record DailyTicketVolume(
    DateTime Date,
    int Count);