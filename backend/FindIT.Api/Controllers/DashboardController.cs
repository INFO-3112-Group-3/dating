using FindIT.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FindIT.Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Handles HTTP GET requests to retrieve dashboard statistics, including counts for Free, Paid, Exposed, and Total Matches.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing the dashboard statistics. The result includes the counts from DashboardStatsDto - { FreeMembersCount, PaidMembersCount, TotalExposedMatches, TotalMatches }.</returns>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _dashboardService.GetManagementStatsAsync();
        return Ok(stats);
    }
}