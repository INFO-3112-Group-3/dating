using FindIT.Api.DTOs;
using FindIT.Api.Entities;
using FindIT.Api.Helpers;
using FindIT.Api.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FindIT.Api.Controllers;

/// <summary>
/// REST Controller for User management. 
/// Handles Authentication, Profile Updates, and Matchmaking requests.
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UsersService _usersService;
    private readonly IGeocodingService _geocodingService;
    private readonly MatchingService _matchingService;

    /// <summary>
    /// Dependencies are injected via the standard ASP.NET Core DI container.
    /// </summary>
    public UsersController(UsersService usersService, IGeocodingService geocodingService, MatchingService matchingService)
    {
        _usersService = usersService;
        _geocodingService = geocodingService;
        _matchingService = matchingService;
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <remarks>The returned user data is mapped to a public data transfer object to prevent exposure of
    /// sensitive information such as password hashes or email addresses.</remarks>
    /// <param name="id">The unique identifier of the user to retrieve. Cannot be null or empty.</param>
    /// <returns>An <see cref="IActionResult"/> containing the user data if found; otherwise, a NotFound result.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _usersService.GetByIdAsync(id);
        if (user == null) return NotFound();

        // Map to Public DTO so we don't leak PasswordHash or Email
        return Ok(user.ToPublicDto());
    }

    /// <summary>
    /// Registers a new user. Performs a basic check for duplicate usernames.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Validation: Prevent duplicate accounts
        if (await _usersService.GetByEmailAsync(request.Email) != null)
            return BadRequest("Username already exists.");

        var newUser = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Salutation = request.Salutation,
            ContactInfo = request.ContactInfo,
            ContactMethod = request.ContactMethod
        };

        //currently doing jack shit
        if (request.Gender == "Male")
        {
            newUser.Gender = Gender.Male;
        }
        if (request.Gender == "Female")
        {
            newUser.Gender = Gender.Female;
        }

        //will calculate age based on brithday entered... for now just setting it to 69 cause funny number
        newUser.Age = 69;

        // Note: Password hashing occurs inside the service layer
        await _usersService.CreateAsync(newUser, request.Password);
        return Ok("Registration successful.");
    }

    /// <summary>
    /// Authenticates a user. 
    /// Securely verifies the password against the stored BCrypt hash.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _usersService.GetByEmailAsync(request.Email);

        // Verify input password against the hashed version in DB
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        // IMPORTANT: We use a Public DTO here to ensure the PasswordHash never leaves the server.
        var userDto = user.ToPublicDto();

        // TODO: Replace "dummy-token-for-now" with a real JWT implementation
        // Not extremely necessary, but if we have time
        return Ok(new AuthResponse { Token = "dummy-token-for-now", User = userDto });
    }

    /// <summary>
    /// Updates user profile details. 
    /// If the City has changed, it automatically re-geocodes the location to update map coordinates.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto updatedDto)
    {
        var existingUser = await _usersService.GetByIdAsync(id);
        if (existingUser is null) return NotFound();

        double[]? newCoords = null;

        // Optimization: Only call the Geocoding API if the location text has actually changed.
        // This saves API credits/rate limits for Nominatim.
        if (updatedDto.City != null && updatedDto.City != existingUser.City)
        {
            var coords = await _geocodingService.GetCoordinatesAsync(updatedDto.City, updatedDto.Region ?? "");
            if (coords.HasValue)
            {
                // MongoDB expects [Longitude, Latitude] for GeoJSON queries
                newCoords = new[] { coords.Value.Lon, coords.Value.Lat };
            }
        }

        await _usersService.UpdateProfileAsync(id, updatedDto, newCoords);
        return Ok("Profile updated successfully.");
    }

    /// <summary>
    /// Deletes the user with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete. Cannot be null.</param>
    /// <returns>A 204 No Content response if the user was successfully deleted; otherwise, a 404 Not Found response if the user
    /// does not exist.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _usersService.GetByIdAsync(id);
        if (user == null) return NotFound();

        await _usersService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Retrieves a ranked list of potential matches for a specific user.
    /// </summary>
    /// <param name="id">The ID of the user seeking matches.</param>
    [HttpGet("{id}/matches")]
    public async Task<IActionResult> GetMatches(string id)
    {
        var currentUser = await _usersService.GetByIdAsync(id);
        if (currentUser == null) return NotFound();

        // Step 1: Query MongoDB for users within the preferred age, gender, and distance.
        // This handles the "hard" requirements.
        var potentialMatches = await _usersService.GetPotentialMatchesAsync(currentUser);

        // Step 2: Use MatchingService to sort the results based on "soft" criteria (Skills/Interests).
        var scores = _matchingService.GetMatches(currentUser, potentialMatches);

        return Ok(scores);
    }
}