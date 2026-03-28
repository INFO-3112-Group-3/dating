using FindIT.Api.Models;
using System.Text.Json.Serialization;

namespace FindIT.Api.DTOs;

// This DTO is used for user registration. It includes necessary fields for creating a new user account.
public class RegisterRequest
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateTime DateOfBirth { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}