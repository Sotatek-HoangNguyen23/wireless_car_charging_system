using API.Services;
using DataAccess.DTOs.UserDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly FeedbackService _feedbackService;

        public FeedbackController(FeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [Authorize("Operator")]
        [HttpGet]
        public IActionResult GetFeedbacks(
            [FromQuery] string? search,
            [FromQuery] string? type,
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = _feedbackService.GetFeedbacks(search, type, status, startDate, endDate, page, pageSize);
            return Ok(result);
        }

        [Authorize("Driver")]
        [HttpPost]
        public IActionResult AddFeedback([FromBody] AddFeedbackDto dto)
        {
            // Kiểm tra Type
            if (string.IsNullOrWhiteSpace(dto.Type) || (dto.Type != "Car" && dto.Type != "Station"))
            {
                return BadRequest(new { message = "Type must be either 'Car' or 'Station'." });
            }

            // Kiểm tra CarId hoặc StationId tùy theo Type
            if (dto.Type == "Car" && dto.CarId == 0)
            {
                return BadRequest(new { message = "CarId is required when Type is 'Car'." });
            }

            if (dto.Type == "Station" && dto.StationId == 0)
            {
                return BadRequest(new { message = "StationId is required when Type is 'Station'." });
            }

            _feedbackService.AddFeedback(dto);
            return Ok(new { message = "Feedback submitted successfully." });
        }

        [Authorize("Operator")]
        [HttpPut("{id}/response")]
        public async Task<IActionResult> ResolveFeedback(int id, [FromBody] UpdateFeedbackStatusDto dto)
        {
            var success = await _feedbackService.ResolveFeedback(id, dto.Status, dto.Response);
            if (!success)
                return NotFound(new { message = "The response does not exist or has been processed." });

            return Ok(new { message = "Feedback ressolved!" });
        }

    }
}
