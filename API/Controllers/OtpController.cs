using API.Services;
using DataAccess.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly OtpServices _otpServices;
        private readonly EmailService _emailService;
        private readonly UserService _userService;
        private readonly string _otpTemplate;

        public OtpController(OtpServices otpServices,EmailService emailService, UserService userService)
        {
            _otpServices = otpServices;
            _emailService = emailService;
            _otpTemplate = System.IO.File.ReadAllText("Template/OTPEmailTemplate.html");
            _userService = userService;
        }
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateOtp([FromBody] OtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage,
                    Status = 400
                });
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
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500,
                });
            }

        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage,
                    Status = 400
                });
            }

            try
            {
                bool isValid = await _otpServices.VerifyOtpAsync(request.Email, request.OtpCode);
                if (isValid)
                {
                    return Ok(new { Message = "OTP verified successfully." });
                }
                return BadRequest(new ProblemDetails
                {
                    Title = "Mã OTP không hợp lệ",
                    Detail = "Mã OTP không đúng hoặc đã hết hạn",
                    Status = 400
                });
            }
            catch
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500,
                });
            }
        }
        [HttpPost("reset-token")]
        public async Task<IActionResult> GenerateTokenReset([FromBody] string Email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage,
                    Status = 400
                });
            }
            try
            {
                var token = await _otpServices.genResetPasswordToken(Email);
                    return Ok(new { Message = "OTP created success", Token=token });
            }
            catch
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500,
                });
            }
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage,
                    Status = 400
                });
            }
            try
            {
                var isValid = await _userService.ResetPassword(request);
                if (isValid)
                {
                    return Ok(new { Message = "Password reset successfully." });
                }
               return BadRequest(new ProblemDetails
               {
                   Title = "Token không hợp lệ",
                   Detail = "Token không đúng hoặc đã hết hạn",
                   Status = 400
               });
            }
            catch
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500,
                });
            }
        }
    }

    
}
