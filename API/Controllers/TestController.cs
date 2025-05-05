using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private TestService _testService;
        private readonly UserService _userService;


        public TestController(TestService testService, UserService userService)
        {
            _testService = testService;
            _userService = userService;
        }
        [AllowAnonymous]
        [EnableRateLimiting("Login")]
        [HttpGet]
        public ActionResult getAllRoles()
        {
            var roles = _testService.GetAllRoles();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return new JsonResult(roles);
        }


        [Authorize("AdminOrOperator")]

        [HttpPost("create-test-user")]
        public async Task<ActionResult> createTestUser([FromBody] CreateTestAccountRequest request)
        {
            try
            {
                await _userService.CreateTestAccount(request.Email, request.Password, request.RoleId);
                return Ok(new { Message = "Test user created successfully." });
            }
            catch (ArgumentException e)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = e.Message,
                    Status = 404
                });
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = e.Message,
                    Status = 400
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." + e.Message,
                    Status = 500
                });
            }
        }

        [Authorize("AdminOrOperator")]
        [HttpDelete("delete-real-user")]
        public async Task<ActionResult> deleteRealUser([FromBody] int userid)
        {
            try
            {
                await _userService.DeleteUserReal(userid);
                return Ok(new { Message = "Real user deleted successfully." });
            }
            catch (ArgumentException e)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = e.Message,
                    Status = 404
                });
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = e.Message,
                    Status = 400
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." + e.Message,
                    Status = 500
                });
            }
        }
        public class CreateTestAccountRequest
        {
            [EmailAddress]
            [StringLength(256)]
            public required string Email { get; set; }

            [StringLength(128, MinimumLength = 8)]
            public required string Password { get; set; }

            [Range(1, 6)]
            public int RoleId { get; set; }
        }
    }
}
