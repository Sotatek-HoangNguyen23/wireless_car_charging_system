using API.Services;
using DataAccess.DTOs.ChargingStation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("system-overview")]
        public async Task<IActionResult> GetSystemOverview()
        {
            var result = await _dashboardService.GetSystemOverviewAsync();
            return Ok(result);
        }


        [HttpGet("charging-sessions")]
        public async Task<IActionResult> GetChargingSessionStats([FromQuery] FilterDto filter)
        {
            var result = await _dashboardService.GetSessionStatistics(filter);
            return Ok(result);
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueStats([FromQuery] FilterDto filter)
        {
            var data = await _dashboardService.GetRevenueStatistics(filter);
            return Ok(data);
        }

        [HttpGet("user")]
        public IActionResult GetUserStats([FromQuery] FilterDto filter)
        {
            var stats = _dashboardService.GetUserStatistics(filter);
            return Ok(stats);
        }
    }
}
