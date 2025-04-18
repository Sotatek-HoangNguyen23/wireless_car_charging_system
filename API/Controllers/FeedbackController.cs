using API.Services;
using DataAccess.DTOs.UserDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly FeedbackService _feedbackService;

        public FeedbackController(FeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

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

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFeedback(int userId)
        {
            var feedbacks = await _feedbackService.GetFeedbackByUserId(userId);
            if (feedbacks == null || feedbacks.Count == 0)
                return NotFound(new { message = "No feedback found" });

            return Ok(feedbacks);
        }

        [HttpPost]
        public IActionResult AddFeedback([FromBody] AddFeedbackDto dto)
        {
            _feedbackService.AddFeedback(dto);
            return Ok(new { message = "Feedback submitted successfully." });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateFeedbackStatusDto dto)
        {
            var success = await _feedbackService.UpdateFeedbackStatusAsync(id, dto.Status);
            if (!success) return NotFound(new { message = "Feedback không tồn tại hoặc đã xử lý." });

            return Ok(new { message = "Cập nhật thành công!" });
        }

    }
}
