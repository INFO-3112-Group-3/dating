using FindIT.Api.Entities;

namespace FindIT.Api.DTOs;

// This DTO is used for updating user profiles. All fields are optional to allow partial updates.
public class UpdateUserDto
{
    public string? Id {get;set;}
    public string Email { get; set; } = null!;
    public bool IsPaidUser { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Salutation {get; set;}
    public string? ContactInfo {get; set;}
    public string? ContactMethod {get; set;}
    public Gender? Gender { get; set; }

    public int Age { get; set; }

    public string? City { get; set; }
    public string? Region { get; set; }

    // GeoJSON Point: [Longitude, Latitude]

    public List<string> Skills { get; set; } = new();


    public UserPreferences? Preferences { get; set; }
}