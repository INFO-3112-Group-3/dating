namespace FindIT.Api.DTOs;
// This DTO is used for sending authentication responses back to the client. It contains the JWT token and the user's public profile information.
public class AuthResponse
{
    public string Token { get; set; } = null!;
    public UserPublicDto User { get; set; } = null!;
}