using FindIT.Api.Models;
using System.Text.Json.Serialization;

namespace FindIT.Api.DTOs;

// This DTO is used for user registration. It includes necessary fields for creating a new user account.
public class RegisterRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateTime DateOfBirth { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Salutation {get; set;}
    public string? ContactInfo {get; set;}
    public string? ContactMethod {get; set;}
    public string? Gender {get; set;}
    //currently a string, and doesn't do jack shit
    public string? Birthday{get;set;}
}