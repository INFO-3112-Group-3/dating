using System.ComponentModel.DataAnnotations;

namespace FindIT.Api.DTOs;

// This DTO represents the data needed to process a payment.
public class PaymentRequest
{
    [Required]
    [CreditCard] // Built-in .NET validation for card formats
    public string CardNumber { get; set; } = null!;

    [Required]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Format must be MM/YY")]
    public string ExpiryDate { get; set; } = null!;

    [Required]
    [StringLength(4, MinimumLength = 3)]
    public string CVV { get; set; } = null!;

    [Range(0.01, 1000.00)]
    public decimal Amount { get; set; }

    [Required]
    public string UserId { get; set; } = null!;
}