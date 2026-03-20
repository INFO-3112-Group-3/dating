namespace FindIT.Api.Models
{
    public class PaymentRequest
    {
        public string CardNumber { get; set; } = null!;
        public string ExpiryDate { get; set; } = null!;
        public string CVV { get; set; } = null!;
        public decimal Amount { get; set; }
        public string UserName { get; set; } = null!;
    }
}
