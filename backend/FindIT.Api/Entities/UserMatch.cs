using MongoDB.Bson.Serialization.Attributes;

namespace FindIT.Api.Entities
{
    public class UserMatch
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? RequesterId { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? TargetId { get; set; }

        public MatchStatus Status { get; set; }

        public int? Rating { get; set; } // star rating 1-5

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    public enum MatchStatus
    {
        Pending,
        Accepted,
        Declined
    }
}
