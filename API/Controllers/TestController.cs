using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public TestController(TestService testService) {
            _testService = testService;
        }
        [Authorize("Admin")]
        [HttpGet]
        public ActionResult getAllRoles()
        {
            var roles = _testService.GetAllRoles();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return new JsonResult(roles);
        }
    }
}
