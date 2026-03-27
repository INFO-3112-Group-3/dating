using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FindIT.Api.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string? Salutation {get; set;}
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool IsPaidUser { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string? ContactInfo {get; set;}
    public string? ContactMethod {get; set;}
    [BsonRepresentation(BsonType.String)]
    public Gender Gender { get; set; } = Gender.NotSpecified;

    public int Age { get; set; }

    public string? City { get; set; }
    public string? Region { get; set; }

    // GeoJSON Point: [Longitude, Latitude]
    public double[] Location { get; set; } = new double[2];

    public List<string> Interests { get; set; } = new();
    public List<string> Skills { get; set; } = new();

    public UserPreferences Preferences { get; set; } = new();
}