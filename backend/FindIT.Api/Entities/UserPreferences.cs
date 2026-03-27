using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FindIT.Api.Entities;

public class UserPreferences
{
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 99;
    public double MaxDistance { get; set; } // in kilometers

    [BsonRepresentation(BsonType.String)]
    public Orientation PreferredOrientation { get; set; } = Orientation.Straight;

    [BsonRepresentation(BsonType.String)]
    public List<Gender> InterestedIn { get; set; } = new();
}