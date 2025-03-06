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
        private readonly EmailService _emailService;
        private readonly string _otpTemplate;

        public OtpController(OtpServices otpServices,EmailService emailService)
        {
            _otpServices = otpServices;
            _emailService = emailService;
            _otpTemplate = System.IO.File.ReadAllText("Template/OTPEmailTemplate.html"); 
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
                var emailBody = _otpTemplate.Replace("{{OTP}}", otp);
                await _emailService.SendEmailAsync(request.Email, "OTP Verification", emailBody);
                return Ok(new { Message = "OTP created success" });
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
