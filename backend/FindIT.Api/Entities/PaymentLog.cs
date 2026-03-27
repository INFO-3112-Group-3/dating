using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FindIT.Api.Entities;

public class PaymentLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRequired]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonRequired]
    public string TransactionId { get; set; } = null!;

    [BsonRequired]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal AmountPaid { get; set; }

    [BsonRequired]
    public DateTime DatePaid { get; set; } = DateTime.UtcNow;

    [BsonRequired]
    [BsonRepresentation(BsonType.String)]
    public PaymentStatus Status { get; set; }
}

public enum PaymentStatus
{
    Pending,
    Success,
    Failed,
    Refunded
}