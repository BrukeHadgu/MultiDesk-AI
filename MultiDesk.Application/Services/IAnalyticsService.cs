using MultiDesk.Application.DTOs.Analytics;

namespace MultiDesk.Application.Services;

public interface IAnalyticsService
{
    Task<DashboardAnalyticsResponse> GetDashboardAsync(
        int tenantId,
        CancellationToken ct = default);
}