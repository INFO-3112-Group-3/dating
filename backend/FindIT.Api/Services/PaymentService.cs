using FindIT.Api.DTOs;
using FindIT.Api.Entities;
using MongoDB.Driver;

namespace FindIT.Api.Services;

/// <summary>
/// Handles financial transactions, subscription lifecycles, and payment validation.
/// Coordinates between PaymentLogs, Subscriptions, and User status updates.
/// </summary>
public class PaymentService
{
    private readonly IMongoCollection<PaymentLog> _paymentCollection;
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IMongoCollection<Subscription> _subscriptionsCollection;

    /// <summary>
    /// Injects MongoDB collections via the database context.
    /// </summary>
    public PaymentService(IMongoDatabase database)
    {
        _usersCollection = database.GetCollection<User>("Users");
        _paymentCollection = database.GetCollection<PaymentLog>("PaymentLogs");
        _subscriptionsCollection = database.GetCollection<Subscription>("Subscriptions");
    }

    /// <summary>
    /// Retrieves all users currently marked as active subscribers.
    /// </summary>
    public async Task<List<Subscription>> GetActiveSubscribersAsync()
    {
        return await _subscriptionsCollection.Find(s => s.IsActive).ToListAsync();
    }

    /// <summary>
    /// Fetches the subscription record for a specific user.
    /// Returns null if the user has never subscribed.
    /// </summary>
    public async Task<Subscription?> GetSubscriptionStatusByIdAsync(string userId)
    {
        return await _subscriptionsCollection
            .Find(s => s.UserId == userId)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves a chronological history of a user's payments, sorted by newest first.
    /// </summary>
    public async Task<List<PaymentLog>> GetPaymentLogsByUserIdAsync(string userId)
    {
        return await _paymentCollection
            .Find(l => l.UserId == userId)
            .SortByDescending(l => l.DatePaid)
            .ToListAsync();
    }

    /// <summary>
    /// Orchestrates the payment process: Validates credentials, logs the attempt, 
    /// and updates the user's subscription status upon success.
    /// </summary>
    /// <param name="request">Contains UserID, Card Details, and Amount.</param>
    /// <returns>True if payment was successful and processed; otherwise false.</returns>
    public async Task<bool> ProcessPaymentAsync(PaymentRequest request)
    {
        var user = await _usersCollection.Find(x => x.Id == request.UserId).FirstOrDefaultAsync();
        if (user == null) return false;

        // Unique identifier for this specific attempt to track in logs/support tickets
        string transactionId = Guid.NewGuid().ToString();

        // 1. Client-side validation check (Luhn, Expiry, CVV)
        if (!ValidatePaymentDetails(request))
        {
            await LogPayment(user.Id!, transactionId, request.Amount, PaymentStatus.Failed);
            return false;
        }

        // 2. Mocking Success: In a production environment, this is where the 
        // Stripe/PayPal/Braintree API call would occur.
        await LogPayment(user.Id!, transactionId, request.Amount, PaymentStatus.Success);

        // 3. Update the Subscription collection (Sets expiry to 1 year from now)
        await UpdateSubscriptionStatus(user.Id!, true, DateTime.UtcNow.AddYears(1));

        // 4. Update the User entity directly. This 'denormalization' allows us to 
        // check "IsPaidUser" without querying the Subscriptions collection every time.
        var userUpdate = Builders<User>.Update.Set(u => u.IsPaidUser, true);
        await _usersCollection.UpdateOneAsync(u => u.Id == user.Id, userUpdate);

        return true;
    }

    /// <summary>
    /// Creates a persistent record of a payment attempt (Success or Failure).
    /// </summary>
    private async Task LogPayment(string userId, string txId, decimal amount, PaymentStatus status)
    {
        var log = new PaymentLog
        {
            UserId = userId,
            TransactionId = txId,
            AmountPaid = amount,
            DatePaid = DateTime.UtcNow,
            Status = status
        };
        await _paymentCollection.InsertOneAsync(log);
    }

    /// <summary>
    /// Synchronizes the subscription status. 
    /// Uses 'Upsert' to create a new record if the user is subscribing for the first time.
    /// </summary>
    private async Task UpdateSubscriptionStatus(string userId, bool active, DateTime expiry)
    {
        var filter = Builders<Subscription>.Filter.Eq(s => s.UserId, userId);
        var update = Builders<Subscription>.Update
            .Set(s => s.IsActive, active)
            .Set(s => s.ExpiryDate, expiry)
            .SetOnInsert(s => s.StartDate, DateTime.UtcNow); // Only sets StartDate if document is NEW

        await _subscriptionsCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    /// <summary>
    /// Aggregated validation logic for credit card payments.
    /// </summary>
    private bool ValidatePaymentDetails(PaymentRequest req)
    {
        if (!IsValidLuhn(req.CardNumber)) return false;
        if (!IsValidExpiryDate(req.ExpiryDate)) return false;
        if (req.CVV.Length < 3 || req.CVV.Length > 4) return false;
        if (req.Amount <= 0) return false;

        return true;
    }

    /// <summary>
    /// Validates the expiration date provided in "MM/YY" format.
    /// Checks if the date is a valid calendar month and is not in the past.
    /// </summary>
    private bool IsValidExpiryDate(string expiryDate)
    {
        if (string.IsNullOrWhiteSpace(expiryDate)) return false;

        var parts = expiryDate.Split('/');
        if (parts.Length != 2) return false;

        if (int.TryParse(parts[0], out int month) && int.TryParse(parts[1], out int year))
        {
            if (month < 1 || month > 12) return false;

            // Map 2-digit year (e.g., 26) to 4-digit (2026)
            int fullYear = 2000 + year;

            // We set expiry to the very last second of the specified month
            var lastDayOfMonth = new DateTime(fullYear, month, DateTime.DaysInMonth(fullYear, month));

            return lastDayOfMonth >= DateTime.UtcNow;
        }

        return false;
    }

    /// <summary>
    /// Performs a checksum validation on the card number using the Luhn Algorithm.
    /// This detects most typing errors and accidental mistranscriptions.
    /// </summary>
    /// <param name="cardNumber">The raw card string (supports spaces and dashes).</param>
    public static bool IsValidLuhn(string cardNumber)
    {
        // Strip formatting characters
        cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 13)
            return false;

        int sum = 0;
        bool alternate = false;

        // Iterate from right to left
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(cardNumber[i])) return false;

            int n = int.Parse(cardNumber[i].ToString());

            if (alternate)
            {
                n *= 2;
                if (n > 9)
                {
                    n -= 9; // Equivalent to adding the digits of the product (e.g., 14 -> 1+4=5)
                }
            }

            sum += n;
            alternate = !alternate;
        }

        // A valid Luhn number must be divisible by 10
        return (sum % 10 == 0);
    }
}