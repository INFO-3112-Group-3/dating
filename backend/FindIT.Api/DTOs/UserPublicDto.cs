using FindIT.Api.Entities;

namespace FindIT.Api.DTOs;

// This DTO is meant for public profiles, so we exclude sensitive info
public class UserPublicDto
{
    public string Id { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? FirstName { get; set; }
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public string? City { get; set; }
    public List<string> Interests { get; set; } = new();
    public List<string> Skills { get; set; } = new();

    // can add last name, email, etc. if needed, but be cautious about privacy
}