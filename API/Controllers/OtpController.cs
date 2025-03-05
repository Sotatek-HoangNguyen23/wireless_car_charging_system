using API.Services;
using DataAccess.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly OtpServices _otpServices;
        public OtpController(OtpServices otpServices)
        {
            _otpServices = otpServices;
        }
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateOtp([FromBody] OtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var otp = await _otpServices.GenerateOtpAsync(request);
                // In a real application, send OTP via email/SMS instead of returning it.
                return Ok(new { OTP = otp });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while generating OTP.");
            }

        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool isValid = await _otpServices.VerifyOtpAsync(request.Email, request.OtpCode);
                if (isValid)
                {
                    return Ok(new { Message = "OTP verified successfully." });
                }
                return BadRequest("Invalid OTP or it has expired.");
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while verifying OTP.");
            }
        }
    }

    
}
