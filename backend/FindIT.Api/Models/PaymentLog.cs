using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FindIT.Api.Models
{
    public class PaymentLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? paymentId { get; set; }

        [BsonRequired]
        public string userID { get; set; }

        [BsonRequired]
        public string transactionID { get; set; }

        [BsonRequired]
        public double amountPaid { get; set; } = 0;

        [BsonRequired]
        public DateTime datePaid { get; set; }

        [BsonRequired]
        public PaymentStatus status { get; set; }
    }

    public enum PaymentStatus
    {
        Success = 0,
        ValidationFailed = 1,
    }
}
