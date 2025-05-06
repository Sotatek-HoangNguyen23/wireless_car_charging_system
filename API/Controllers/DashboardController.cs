using API.Services;
using DataAccess.DTOs.ChargingStation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [Authorize("Operator")]
        [HttpGet("system-overview")]
        public async Task<IActionResult> GetSystemOverview()
        {
            var result = await _dashboardService.GetSystemOverviewAsync();
            return Ok(result);
        }

        [Authorize("StationOwnerOrOperator")]
        [HttpGet("charging-sessions")]
        public async Task<IActionResult> GetChargingSessionStats([FromQuery] FilterDto filter)
        {
            var userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);

            var result = await _dashboardService.GetSessionStatistics(filter, userId, role);
            return Ok(result);
        }

        [Authorize("StationOwnerOrOperator")]
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueStats([FromQuery] FilterDto filter)
        {
            var userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);

            var data = await _dashboardService.GetRevenueStatistics(filter, userId, role);
            return Ok(data);
        }

        [Authorize("Operator")]
        [HttpGet("user")]
        public IActionResult GetUserStats([FromQuery] FilterDto filter)
        {
            var stats = _dashboardService.GetUserStatistics(filter);
            return Ok(stats);
        }
    }
}
