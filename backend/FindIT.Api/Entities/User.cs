using FindIT.Api.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace FindIT.Api.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? Salutation { get; set; } // "Mr.", "Ms.", "Dr."
    public string? Nickname { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool IsPaidUser { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Gender Gender { get; set; } = Gender.NotSpecified;
    [BsonRequired]
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateTime DateOfBirth { get; set; }

    // Calculated property - automatically calculates age based on date of birth
    [BsonIgnore]
    public int Age
    {
        get
        {
            DateTime today = DateTime.UtcNow;
            int age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    public string? ContactMethod { get; set; }
    public string? ContactInfo { get; set; }

    public string? City { get; set; }
    public string? Region { get; set; }

    // GeoJSON Point: [Longitude, Latitude]
    public double[] Location { get; set; } = new double[2];

    public List<string> Interests { get; set; } = new();
    public List<string> Skills { get; set; } = new();

    public List<UserPreference> Preferences { get; set; } = new();

    public string? Bio { get; set; } = "";
    public string? ProfilePictureBase64 { get; set; }
}