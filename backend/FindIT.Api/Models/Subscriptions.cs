using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FindIT.Api.Models
{
    public class Subscriptions
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? subscriptionId { get; set; }

        [BsonRequired]
        public string userID { get; set; }

        [BsonRequired]
        public DateTime startDate { get; set; }

        [BsonRequired]
        public DateTime expiryDate { get; set; }

        public bool isActive { get; set; } = true;
    }
}
