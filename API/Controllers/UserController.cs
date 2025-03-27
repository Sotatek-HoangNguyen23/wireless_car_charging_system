using API.Services;
using DataAccess.DTOs.UserDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("email")]
        public async Task<IActionResult> GetUserByEmail([FromBody] EmailRequest emailRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emailRequest.Email))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Validation Error",
                        Detail = "Email là bắt buộc",
                        Status = 400,
                        Extensions = { ["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
                    });
                }

                var user = await _userService.GetUserByEmail(emailRequest.Email);

                if (user == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Not Found",
                        Detail = $"Không tìm thấy người dùng với email {emailRequest.Email}",
                        Status = 404,
                        Extensions = { ["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier }

                    });
                }

                return Ok(user);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Type = "Server Error",
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500,
                    Extensions = { ["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
                });
            }
        }

        [HttpGet]
        public IActionResult GetUsers(
            [FromQuery] string? searchQuery,
            [FromQuery] string? status,
            [FromQuery] int? roleId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var result = _userService.GetUsers(searchQuery, status, roleId, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpPut("change-status/{userId}")]
        public async Task<IActionResult> ChangeUserStatus(int userId, [FromBody] string newStatus)
        {
            await _userService.ChangeUserStatusAsync(userId, newStatus);
            return Ok(new { message = "User status updated successfully." });
        }

        [HttpGet("Feedback")]
        public IActionResult GetFeedbacks(
            [FromQuery] string? search,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = _userService.GetFeedbacks(search, startDate, endDate, page, pageSize);
            return Ok(result);
        }

        [HttpGet("Feedback/{userId}")]
        public async Task<IActionResult> GetFeedback(int userId)
        {
            var feedbacks = await _userService.GetFeedbackByUserId(userId);
            if (feedbacks == null || feedbacks.Count == 0)
                return NotFound(new { message = "No feedback found" });

            return Ok(feedbacks);
        }
    }
}
