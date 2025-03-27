using API.Services;
using DataAccess.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController()
        {
            _paymentService = new PaymentService();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] DepositDTO request)
        {
            try
            {
                var checkoutUrl = await _paymentService.CreatePaymentLink(request);
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
            await _paymentService.HandlePaymentCallback(orderCode, status);
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

        [HttpGet("balance/{userId}")]
        public async Task<IActionResult> GetBalanceByUserId([FromRoute] int userId)
        {
            var balance = await _paymentService.GetBalanceByUserId(userId);

            if (balance == null)
            {
                return NotFound(new { message = "Balance not found" });
            }

            return Ok(balance);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactionHistory(
        [FromQuery] int userId,
        
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end)
        {
            var transactions = await _paymentService.GetTransactionHistory(userId, start, end);

            if (transactions == null || transactions.Count == 0)
                return NotFound(new { message = "No transactions found" });

            return Ok(transactions);
        }


    }
}
