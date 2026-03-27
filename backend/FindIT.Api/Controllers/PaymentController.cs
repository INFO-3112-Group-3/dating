using FindIT.Api.DTOs;
using FindIT.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FindIT.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("subscribers/active")]
    public async Task<IActionResult> GetAllActiveSubscribers()
    {
        var results = await _paymentService.GetActiveSubscribersAsync();
        return Ok(results);
    }

    [HttpGet("status/{userId}")]
    public async Task<IActionResult> GetSubscriptionStatus(string userId)
    {
        var result = await _paymentService.GetSubscriptionStatusByIdAsync(userId);
        return result is null ? NotFound("No active subscription found.") : Ok(result);
    }

    [HttpGet("logs/{userId}")]
    public async Task<IActionResult> GetPaymentLogs(string userId)
    {
        var result = await _paymentService.GetPaymentLogsByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
    {
        // The [ApiController] attribute automatically handles basic 400 errors 
        // for DataAnnotations in the DTO, but we check our logic here.
        var success = await _paymentService.ProcessPaymentAsync(request);

        if (!success)
        {
            return BadRequest(new { message = "Payment failed. Please check your card details." });
        }

        return Ok(new { message = "Payment successful! Subscription activated." });
    }
}