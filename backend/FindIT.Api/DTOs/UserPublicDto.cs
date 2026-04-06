using FindIT.Api.Entities;

namespace FindIT.Api.DTOs;

// This DTO is meant for public profiles, so we exclude sensitive info
public class UserPublicDto
{
    public string? Id {get;set;}
    public string? Nickname {get;set;}
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

    public DateTime DateOfBirth {get;set;}

    // GeoJSON Point: [Longitude, Latitude]
    public List<string> Interests { get; set; } = new();
    public List<string> Skills { get; set; } = new();

    public List<UserPreference> Preferences {get; set;} = new();

    public string? Bio { get; set; }
    public string? ProfilePictureBase64 { get; set; }
  // can add last name, email, etc. if needed, but be cautious about privacy
}