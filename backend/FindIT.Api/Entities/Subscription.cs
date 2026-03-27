using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Subscription
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}