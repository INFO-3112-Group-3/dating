using FindIT.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FindIT.Api.Services
{
    public class PaymentServices
    {
        private readonly IMongoCollection<PaymentLog> _paymentCollection;
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Subscriptions> _subscriptionsCollection;

        public PaymentServices( IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<User>(
                databaseSettings.Value.UsersCollectionName);
            _paymentCollection = mongoDatabase.GetCollection<PaymentLog>(
                databaseSettings.Value.PaymentCollectionName);
            _subscriptionsCollection = mongoDatabase.GetCollection<Subscriptions>(
                databaseSettings.Value.SubscriptionsCollectionName);
        }



        // Get all subscriptioner with active subscription status
        public async Task<List<Subscriptions>> GetActiveSubscribersAsync()
        {
            return await _subscriptionsCollection.Find(x => x.isActive).ToListAsync();
        }

        // Get user status from subscriptions ( can be null if user has no subscription record or is not active )
        public async Task<Subscriptions?> GetSubscriptionsByUserNameAsync(string userName)
        {
            // get userId from userName
            var user = await _usersCollection.Find(x => x.Username == userName).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }

            return await _subscriptionsCollection.Find(x => x.userID == user.Id && x.isActive).FirstOrDefaultAsync();
        }

        // Get payment log by user ID ( return empty list if no payment log found )
        public async Task<List<PaymentLog>> GetPaymentLogsByUserNameAsync(string userName)
        {
            var user = await _usersCollection.Find(x => x.Username == userName).FirstOrDefaultAsync();
            if (user == null)
            {
                return new List<PaymentLog>();
            }

            return await _paymentCollection.Find(x => x.userID == user.Id).ToListAsync();
        }

        // Process a payment (validate -> logging -> update subscription status) -> return boolean for success or failure
        public async Task<bool> ProcessPaymentAsync(PaymentRequest paymentRequest)
        {
            var user = await _usersCollection.Find(x => x.Username == paymentRequest.UserName).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Validate payment details, if failed, log the failed payment and return false
            if (!ValidatePaymentDetails(paymentRequest.CardNumber, paymentRequest.ExpiryDate, paymentRequest.CVV, paymentRequest.Amount, user.Id))
            {
                await LogPayment(user.Id, Guid.NewGuid().ToString(), paymentRequest.Amount, PaymentStatus.ValidationFailed);
                return false;
            }

            // if validation passed -> payment go through
            bool paymentSuccess = true;


            // Log the payment result
            await LogPayment(user.Id, Guid.NewGuid().ToString(), paymentRequest.Amount, PaymentStatus.Success);

            // Update subscription status if payment is successful
            if (paymentSuccess)
            {
                await UpdateSubscriptionStatus(user.Id, true, DateTime.UtcNow.AddMonths(12)); // Assuming 1-year subscription
            }

            return true;
        }


        // helper method to validate payment details
        private bool ValidatePaymentDetails(string cardNumber, string expiryDate, string cvv, decimal amount, string userId)
        {
            // Validate the Card Number
            if (!IsValidLuhn(cardNumber)) return false;

            // Validate Expiry Date (Format: MM/YY)
            if (!IsValidExpiryDate(expiryDate)) return false;

            // Validate CVV (Standard 3 or 4 digits)
            if (string.IsNullOrWhiteSpace(cvv) || (cvv.Length < 3 || cvv.Length > 4)) return false;

            // Validate Amount
            if (amount <= 0) return false;

            // User ID should not be empty
            if (string.IsNullOrWhiteSpace(userId)) return false;

            return true;
        }

        // helper method to validate expiry date
        private bool IsValidExpiryDate(string expiryDate)
        {
            if (string.IsNullOrWhiteSpace(expiryDate)) return false;

            var parts = expiryDate.Split('/');
            if (parts.Length != 2) return false;

            if (int.TryParse(parts[0], out int month) && int.TryParse(parts[1], out int year))
            {
                if (month < 1 || month > 12) return false;

                int fullYear = 2000 + year; // Assuming cards won't have expiry beyond 2099
                // Get the last day of the expiry month
                var lastDayOfMonth = new DateTime(fullYear, month, DateTime.DaysInMonth(fullYear, month));

                return lastDayOfMonth >= DateTime.UtcNow;
            }

            return false;
        }

        // helper method to validate card number using Luhn algorithm
        public static bool IsValidLuhn(string cardNumber)
        {
            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 13)
                return false;

            int sum = 0;
            bool alternate = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(cardNumber[i])) return false;

                int n = int.Parse(cardNumber[i].ToString());

                if (alternate)
                {
                    n *= 2;
                    if (n > 9)
                    {
                        n -= 9;
                    }
                }

                sum += n;
                alternate = !alternate;
            }

            return (sum % 10 == 0);
        }

        // helper method to update user subscription status
        private async Task UpdateSubscriptionStatus(string userId, bool isActive, DateTime? expiryDate = null)
        {
            var subscription = await _subscriptionsCollection.Find(x => x.userID == userId).FirstOrDefaultAsync();
            if (subscription != null)
            {
                subscription.isActive = isActive;
                if (expiryDate.HasValue)
                {
                    subscription.expiryDate = expiryDate.Value;
                }
                // update existing subscription
                await _subscriptionsCollection.UpdateOneAsync(x => x.subscriptionId == subscription.subscriptionId, Builders<Subscriptions>.Update
                    .Set(s => s.isActive, subscription.isActive)
                    .Set(s => s.expiryDate, subscription.expiryDate));
            }
            else
            {
                // If no existing subscription, create a new one
                var newSubscription = new Subscriptions
                {
                    userID = userId,
                    startDate = DateTime.UtcNow,
                    expiryDate = expiryDate ?? DateTime.UtcNow.AddMonths(12), // Default to 1 year subscription if expiry date not provided
                    isActive = isActive
                };
                await _subscriptionsCollection.InsertOneAsync(newSubscription);
            }
        }

        // helper method to create a new payment log entry
        private async Task LogPayment(string userId, string transactionId, decimal amount, PaymentStatus status)
        {
            var paymentLog = new PaymentLog
            {
                userID = userId,
                transactionID = transactionId,
                amountPaid = (double)amount,
                datePaid = DateTime.UtcNow,
                status = status
            };
            await _paymentCollection.InsertOneAsync(paymentLog);
        }

    }
}
