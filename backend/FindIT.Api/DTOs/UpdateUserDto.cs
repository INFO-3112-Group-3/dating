using FindIT.Api.Entities;
using FindIT.Api.Models;
using System.Text.Json.Serialization;

namespace FindIT.Api.DTOs;

// This DTO is used for updating user profiles. All fields are optional to allow partial updates.
public class UpdateUserDto
{
    public string? Salutation { get; set; }
    public string? Nickname { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Gender? Gender { get; set; }
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateTime? DateOfBirth { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public ContactMethod? PreferredContact { get; set; }
    public string? ContactIdentifier { get; set; }

    public UserPreferences? Preferences { get; set; }
}