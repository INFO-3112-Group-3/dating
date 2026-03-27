using FindIT.Api.Entities;
using FindIT.Api.Models;
using System.Text.Json.Serialization;

namespace FindIT.Api.DTOs;

// This DTO is meant for public profiles, so we exclude sensitive info
public class UserPublicDto
{
    public string Id { get; set; } = null!;
    public string? Salutation { get; set; } // "Mr.", "Ms.", "Dr."
    public string? Nickname { get; set; }
    public string Username { get; set; } = null!;
    public string? FirstName { get; set; }
    public Gender Gender { get; set; }
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateTime DateOfBirth { get; set; }
    public int Age  { get; set; }
    public ContactMethod PreferredContact { get; set; } = ContactMethod.Email;
    public string? ContactIdentifier { get; set; }
    public string? City { get; set; }
    public List<string> Interests { get; set; } = new();
    public List<string> Skills { get; set; } = new();

    // can add last name, email, etc. if needed, but be cautious about privacy
}