using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FindIT.Api.Models
{
    public class SkillTags
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Name { get; set; } = null!;
        public string Category { get; set; } = "General"; // "Technical", "Soft Skill", ...
    }
}
