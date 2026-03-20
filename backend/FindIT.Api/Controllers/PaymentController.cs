using FindIT.Api.Models;
using FindIT.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FindIT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentServices _context;

        public PaymentController(PaymentServices context)
        {
            _context = context;
        }

        // GET: api/payment/active-subscribers : Returns all active subscribers
        [HttpGet("active-subscribers")]
        public async Task<IActionResult> GetAllActiveSubscribers()
        {
            var results = await _context.GetActiveSubscribersAsync();
            return Ok(results);
        }

        // GET: api/payment/subscription-status/{userName} : Returns subscription status for a user
        [HttpGet("subscription-status/{userName}")]
        public async Task<IActionResult> GetSubscriptionStatus(string userName)
        {
            var result = await _context.GetSubscriptionsByUserNameAsync(userName);
            return Ok(result);
        }

        // GET: api/payment/payment-logs/{userName} : Returns payment logs for a user
        [HttpGet("payment-logs/{userName}")]
        public async Task<IActionResult> GetPaymentLogs(string userName)
        {
            var result = await _context.GetPaymentLogsByUserNameAsync(userName);
            return Ok(result);
        }

        // POST: api/payment/process : Process a payment
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest paymentRequest)
        {
            var result = await _context.ProcessPaymentAsync(paymentRequest);
            return Ok(result);
        }

    }
}
