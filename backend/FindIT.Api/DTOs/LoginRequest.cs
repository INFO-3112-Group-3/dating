namespace FindIT.Api.DTOs
{
    // This DTO is used for login requests. It contains the necessary information for a user to authenticate.
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
