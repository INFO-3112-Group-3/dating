using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FindIT.Api.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("Name")]
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool IsPaidUser { get; set; } = false;
    public string? City { get; set; } = null!;
    public string? Region { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? FirstName { get; set; } = null!;
    public string? LastName { get; set; } = null!;
    public string? Gender { get; set; } = null!;
    public List<string> Interests { get; set; } = new List<string>();
    public string? Occupation { get; set; } = null!;
    public string? Orientation { get; set; } = null!;
    public List<string> Skills { get; set; } = new List<string>();
    public string? Notes { get; set; } = null!;
}