using API.Services;
using DataAccess.DTOs.ChargingStation;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingStationController : ControllerBase
    {
        private ChargingStationService _stationService;

        public ChargingStationController(ChargingStationService stationService)
        {
            _stationService = stationService;
        }

        [HttpGet]
        public ActionResult GetChargingStations(string? keyword, decimal userLat, decimal userLng, int page = 1, int pageSize = 2)
        {
            var stations = _stationService.GetChargingStations(keyword, userLat, userLng, page, pageSize);
            return new JsonResult(stations);
        }


        [HttpGet("Detail/{stationId}")]
        public ActionResult GetStationDetails(int stationId, int page, int pageSize)
        {
            var stationDetails = _stationService.GetStationDetails(stationId, page, pageSize);

            if (stationDetails == null)
            {
                return NotFound(new { message = "Station not found" });
            }

            return Ok(stationDetails);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddStation([FromBody] NewChargingStationDto stationDto)
        {
            var result = await _stationService.AddChargingStation(stationDto);
            if (result)
                return Ok(new { message = "Add Station Successfully!" });
            return BadRequest(new { message = "Error." });
        }

        [HttpDelete("{stationId}")]
        public async Task<IActionResult> DeleteChargingStation(int stationId)
        {
            var result = await _stationService.DeleteChargingStation(stationId);
            if (!result)
                return NotFound(new { message = "Station does not exist!" });

            return Ok(new { message = "Station deleted Successfully!" });
        }


        [HttpPut("{stationId}")]
        public async Task<IActionResult> UpdateChargingStation(int stationId, [FromBody] UpdateChargingStationDto stationDto)
        {
            var updatedStation = await _stationService.UpdateChargingStation(stationId, stationDto);
            if (updatedStation == null)
                return NotFound(new { message = "Station does not exist!" });

            return Ok(updatedStation);
        }

        [HttpGet("{stationId}/stats")]
        public IActionResult GetStationStats(int stationId, int? year, int? month)
        {
            var stats = _stationService.GetStats(stationId, year, month);
            return Ok(stats);
        }
    }
}
