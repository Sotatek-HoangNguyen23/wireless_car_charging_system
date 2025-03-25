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
                    });
                }

                return Ok(user);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500
                });
            }
        }
    }
}
