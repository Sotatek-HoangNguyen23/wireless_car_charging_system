using API.Services;
using DataAccess.DTOs.ChargingStation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingStationController : ControllerBase
    {
        private readonly ChargingStationService _stationService;

        public ChargingStationController(ChargingStationService stationService)
        {
            _stationService = stationService;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetChargingStations(string? keyword, decimal? userLat, decimal? userLng, int page = 1, int pageSize = 2)
        {
            var currentUserId = 0;
            var role = "";
            // Nếu không phải Anonymous thì mới lấy userId
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                currentUserId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                role = User.FindFirstValue(ClaimTypes.Role);
            }

            var stations = _stationService.GetChargingStations(keyword, userLat, userLng, page, pageSize, role, currentUserId);
            return new JsonResult(stations);
        }

        [AllowAnonymous]
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

        [Authorize("StationOwnerOrOperator")]
        [HttpPost("Add")]
        public async Task<IActionResult> AddStation([FromBody] NewChargingStationDto stationDto)
        {
            // Kiểm tra validate các trường
            var validationErrors = ValidateStation(stationDto);
            if (validationErrors.Any())
            {
                return BadRequest(new { message = "Validation failed.", errors = validationErrors });
            }

            var result = await _stationService.AddChargingStation(stationDto);
            if (result)
                return Ok(new { message = "Add Station Successfully!" });

            return BadRequest(new { message = "Error." });
        }

        private List<string> ValidateStation(NewChargingStationDto stationDto)
        {
            var errors = new List<string>();

            // Validate stationName
            if (string.IsNullOrEmpty(stationDto.StationName) || stationDto.StationName.Length > 225)
            {
                errors.Add("Tên trạm sạc không vượt quá 225 ký tự");
            }

            // Validate stationAddress
            if (string.IsNullOrEmpty(stationDto.Address) || stationDto.Address.Length > 225)
            {
                errors.Add("Địa chỉ không vượt quá 225 ký tự");
            }

            // Validate description
            //if (string.IsNullOrEmpty(stationDto.LocationDescription) || stationDto.LocationDescription.Length > 225)
            //{
            //    errors.Add("Mô tả không vượt quá 225 ký tự");
            //}

            // Validate pointDescription
            if (string.IsNullOrEmpty(stationDto.PointDescription) || stationDto.PointDescription.Length > 225)
            {
                errors.Add("Mô tả điểm sạc không vượt quá 225 ký tự");
            }

            // Validate totalPoints
            if (stationDto.TotalPoint <= 0 || stationDto.TotalPoint > 100)
            {
                errors.Add("Số điểm sạc tối đa là 100");
            }

            // Validate pointName
            if (string.IsNullOrEmpty(stationDto.PointCode) || !Regex.IsMatch(stationDto.PointCode, "^[A-Za-z]{1,5}$"))
            {
                errors.Add("Mã điểm sạc chỉ chứa tối đa 5 chữ cái");
            }

            // Validate maxPower
            if (stationDto.MaxPower <= 0 || stationDto.MaxPower > 350)
            {
                errors.Add("Công suất tối đa là 350kW");
            }

            return errors;
        }


        [Authorize("StationOwnerOrOperator")]
        [HttpDelete("{stationId}")]
        public async Task<IActionResult> DeleteChargingStation(int stationId)
        {
            var result = await _stationService.DeleteChargingStation(stationId);
            if (result == null)
                return NotFound(new { message = "Station does not exist!" });

            return Ok(new { message = "Station deleted Successfully!" });
        }

        [Authorize("StationOwnerOrOperator")]
        [HttpPut("{stationId}")]
        public async Task<IActionResult> UpdateChargingStation(int stationId, [FromBody] UpdateChargingStationDto stationDto)
        {
            if (string.IsNullOrEmpty(stationDto.StationName) || stationDto.StationName.Length > 225)
            {
                return BadRequest(new { message = "Charging station name cannot exceed 225 character!" });
            }

            try
            {
                var updatedStation = await _stationService.UpdateChargingStation(stationId, stationDto);

                return Ok(new { message = "Station updated Successfully!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            
        }

        [Authorize("StationOwnerOrOperator")]
        [HttpGet("{stationId}/stats")]
        public IActionResult GetStationStats(int stationId, int? year, int? month)
        {
            if (year.HasValue && (year < 1900 || year > DateTime.Now.Year))
            {
                return BadRequest(new { message = "Invalid year value!" });
            }

            if (month.HasValue && (month < 1 || month > 12))
            {
                return BadRequest(new { message = "Invalid month value!" });
            }
            try
            {
                var stats = _stationService.GetStats(stationId, year, month);
                return Ok(stats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


    }
}
