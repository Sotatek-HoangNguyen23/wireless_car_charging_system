using API.Services;
using DataAccess.DTOs.ChargingStation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingPointController : ControllerBase
    {

        private readonly ChargingStationService _stationService;

        public ChargingPointController(ChargingStationService stationService)
        {
            _stationService = stationService;
        }

        [AllowAnonymous]
        [HttpGet("{pointId}")]
        public ActionResult GetChargingPointDetail(int pointId) 
        {
            var point = _stationService.GetPointById(pointId);
            if (point == null)
            {
                return NotFound(new { message = "Charging Point not found!" });
            }

            return Ok(point);
        }

        [Authorize("StationOwnerOrOperator")]
        [HttpPost("AddPoints")]
        public async Task<IActionResult> AddPoints(int stationId, [FromBody] NewChargingStationDto pointDto)
        {
            if (pointDto == null || stationId <= 0)
            {
                return BadRequest("Invalid data");
            }

            var validationErrors = ValidatePoint(pointDto);
            if (validationErrors.Any())
            {
                return BadRequest(new { message = "Validation failed.", errors = validationErrors });
            }

            var points = await _stationService.AddPoint(stationId, pointDto);

            if (points == null || !points.Any())
            {
                return BadRequest("Cannot add charging points!");
            }

            return Ok(new { Message = "Point added successfully!", Points = points });
        }

        private List<string> ValidatePoint(NewChargingStationDto stationDto)
        {
            var errors = new List<string>();

            // Validate pointDescription
            if (string.IsNullOrEmpty(stationDto.PointDescription) || stationDto.PointDescription.Length > 225)
            {
                errors.Add("Description of charging point not exceeding 225 characters");
            }

            // Validate totalPoints
            if (stationDto.TotalPoint <= 0 || stationDto.TotalPoint > 100)
            {
                errors.Add("Maximum number of charging points is 100");
            }

            // Validate pointName
            if (string.IsNullOrEmpty(stationDto.PointCode) || !Regex.IsMatch(stationDto.PointCode, "^[A-Za-z]{1,5}$"))
            {
                errors.Add("Charging point code contains maximum 5 letters only");
            }

            // Validate maxPower
            if (stationDto.MaxPower <= 0 || stationDto.MaxPower > 350)
            {
                errors.Add("Maximum power is 350kW");
            }

            return errors;
        }

        [Authorize("StationOwnerOrOperator")]
        [HttpDelete("{pointId}")]
        public async Task<IActionResult> DeleteChargingPoint(int pointId)
        {         
            var result = await _stationService.DeleteChargingPoint(pointId);  
            if(result == null )
            {
                return NotFound(new { message = "Charging Point not found!" });
            }

            return Ok(new { message = "Point deleted Successfully!" });
        }

        [Authorize("StationOwnerOrOperator")]
        [HttpPut("{pointId}")]
        public async Task<IActionResult> UpdateChargingPoint(int pointId, [FromBody] UpdateChargingPointDto pointDto)
        {
            var updatedPoint= await _stationService.UpdateChargingPoint(pointId, pointDto);
            if (updatedPoint == null)
                return NotFound(new { message = "Charging Point not found!" });

            return Ok(updatedPoint);
        }
    }
}
