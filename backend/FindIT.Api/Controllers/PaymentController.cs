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

    /// <summary>
    /// Retrieves all currently active subscribers.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing a collection of active subscribers. The result is an HTTP 200 response
    /// with the list of subscribers if successful; otherwise, an appropriate error response.</returns>
    [HttpGet("subscribers/active")]
    public async Task<IActionResult> GetAllActiveSubscribers()
    {
        var results = await _paymentService.GetActiveSubscribersAsync();
        return Ok(results);
    }

    /// <summary>
    /// Retrieves the subscription status for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose subscription status is to be retrieved. Cannot be null or empty.</param>
    /// <returns>An <see cref="IActionResult"/> containing the subscription status if found; otherwise, a NotFound result if the
    /// user has no active subscription.</returns>
    [HttpGet("status/{userId}")]
    public async Task<IActionResult> GetSubscriptionStatus(string userId)
    {
        var result = await _paymentService.GetSubscriptionStatusByIdAsync(userId);
        return result is null ? NotFound("No active subscription found.") : Ok(result);
    }

    /// <summary>
    /// Retrieves the collection of payment logs associated with the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose payment logs are to be retrieved. Cannot be null.</param>
    /// <returns>An <see cref="IActionResult"/> containing the payment logs for the specified user. Returns an empty collection
    /// if no logs are found.</returns>
    [HttpGet("logs/{userId}")]
    public async Task<IActionResult> GetPaymentLogs(string userId)
    {
        var result = await _paymentService.GetPaymentLogsByUserIdAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Processes a payment request and activates the subscription if the payment is successful.
    /// </summary>
    /// <remarks>The [ApiController] attribute automatically handles validation errors for the request model.
    /// This method returns user-friendly messages for both success and failure scenarios.</remarks>
    /// <param name="request">The payment details to process. Must include all required payment information. Cannot be null.</param>
    /// <returns>An IActionResult indicating the result of the payment operation. Returns 200 OK with a success message if the
    /// payment is processed successfully; otherwise, returns 400 Bad Request with an error message.</returns>
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