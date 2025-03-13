using API.Services;
using DataAccess.DTOs.ChargingStation;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingPointController : ControllerBase
    {

        private ChargingStationService _stationService;

        public ChargingPointController(ChargingStationService stationService)
        {
            _stationService = stationService;
        }

        [HttpGet("{pointId}")]
        public ActionResult GetChargingPointDetail(int pointId) 
        {
            var point = _stationService.GetPointById(pointId);
            if (point == null)
            {
                return NotFound(new { message = "Charging Point not found" });
            }

            return Ok(point);
        }

        [HttpPost("AddPoints")]
        public async Task<IActionResult> AddPoints(int stationId, [FromBody] NewChargingStationDto pointDto)
        {
            if (pointDto == null || stationId <= 0)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            var points = await _stationService.AddPoint(stationId, pointDto);

            if (points == null || !points.Any())
            {
                return BadRequest("Không thể thêm điểm sạc.");
            }

            return Ok(new { Message = "Thêm điểm sạc thành công!", Points = points });
        }


        [HttpDelete("{pointId}")]
        public async Task<IActionResult> DeleteChargingPoint(int pointId)
        {
            var result = await _stationService.DeleteChargingPoint(pointId);
            if (!result)
                return NotFound(new { message = "Point does not exist!" });

            return Ok(new { message = "Point deleted Successfully!" });
        }


        [HttpPut("{pointId}")]
        public async Task<IActionResult> UpdateChargingPoint(int pointId, [FromBody] UpdateChargingPointDto pointDto)
        {
            var updatedPoint= await _stationService.UpdateChargingPoint(pointId, pointDto);
            if (updatedPoint == null)
                return NotFound(new { message = "Point does not exist!" });

            return Ok(updatedPoint);
        }
    }
}
