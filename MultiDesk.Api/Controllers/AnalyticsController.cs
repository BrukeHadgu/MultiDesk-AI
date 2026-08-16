using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiDesk.Api.Extensions;
using MultiDesk.Application.DTOs.Analytics;
using MultiDesk.Application.Services;

namespace MultiDesk.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "Admin,Agent")]
[Produces("application/json")]
[Tags("Analytics")]
public class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardAnalyticsResponse), StatusCodes.Status200OK)]
    [EndpointSummary("Get dashboard analytics for the tenant")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var result   = await analyticsService.GetDashboardAsync(tenantId, ct);
        return Ok(result);
    }
}