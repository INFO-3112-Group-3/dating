using FindIT.Api.Entities;

namespace FindIT.Api.DTOs;

// This DTO is used for updating user profiles. All fields are optional to allow partial updates.
public class UpdateUserDto
{
    public string? Nickname {get;set;}
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Salutation {get; set;}
    public string? ContactInfo {get; set;}
    public string? ContactMethod {get; set;}
    public Gender? Gender { get; set; }

    public string? City { get; set; }
    public string? Region { get; set; }

    public List<string> Interests { get; set; } = new();
    public List<string> Skills { get; set; } = new();

    public List<UserPreference>? Preferences {get;set;} = new();
}