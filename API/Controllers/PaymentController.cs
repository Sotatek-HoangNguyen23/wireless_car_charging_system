using API.Services;
using DataAccess.DTOs.CarDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] DepositDTO request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Bạn cần đăng nhập để thực hiện thao tác này",
                    Status = 401
                });
            }
            int userId = int.Parse(userIdClaim.Trim());
            try
            {
                var checkoutUrl = await _paymentService.CreatePaymentLink(request,userId);
                return Ok(new { checkoutUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("callback")]
        public async Task<IActionResult> PaymentCallback([FromQuery] int orderCode, [FromQuery] string status)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Bạn cần đăng nhập để thực hiện thao tác này",
                    Status = 401
                });
            }
            int userId = int.Parse(userIdClaim.Trim());
            await _paymentService.HandlePaymentCallback(orderCode, status,userId);
            return Ok(new { message = "Updated", status });
        }

        [HttpGet("status/{orderId}")]
        public async Task<IActionResult> GetStatus([FromRoute] int orderId)
        {
            var payment = await _paymentService.GetPaymentStatus(orderId);
            return payment == null
                ? NotFound()
                : Ok(new { status = payment.Status });
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalanceByUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Bạn cần đăng nhập để thực hiện thao tác này",
                    Status = 401
                });
            }
            int userId = int.Parse(userIdClaim.Trim());
            var balance = await _paymentService.GetBalanceByUserId(userId);

            if (balance == null)
            {
                return NotFound(new { message = "Balance not found" });
            }

            return Ok(balance);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactionHistory(
        
        
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Bạn cần đăng nhập để thực hiện thao tác này",
                    Status = 401
                });
            }
            int userId = int.Parse(userIdClaim.Trim());

            var transactions = await _paymentService.GetTransactionHistory(userId, start, end);

            if (transactions == null || transactions.Count == 0)
                return NotFound(new { message = "No transactions found" });

            return Ok(transactions);
        }


    }
}
