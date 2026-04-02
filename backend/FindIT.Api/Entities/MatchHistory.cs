using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FindIT.Api.Entities;

public class MatchHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserAId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserBId { get; set; } = null!;

    public int MatchScore { get; set; }

    // Tracks if contact info was shown
    public bool CommunicationExposed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}