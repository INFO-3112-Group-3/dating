using FindIT.Api.Models;
using FindIT.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FindIT.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UsersService _context;

    public UsersController(UsersService context)
    {
        _context = context;
    }

    // GET: api/users : Returns all users
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _context.GetAll());

    // GET: api/users/username/{username} : Returns a user by their username
    [HttpGet("username/{username}")]
    public async Task<IActionResult> GetByUser(string username)
    {
        var user = await _context.GetUserByUsername(username);
        return user is null ? NotFound() : Ok(user);
    }

    // GET: api/users/email/{email} : Returns a user by their email
    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var user = await _context.GetUserByEmail(email);
        return user is null ? NotFound() : Ok(user);
    }

    // POST: api/users : Creates a new user
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User newUser)
    {
        // check if username or email already exists
        if (await _context.GetUserByUsername(newUser.Username) != null)
            return BadRequest("Username already exists.");
        if (await _context.GetUserByEmail(newUser.Email) != null)
            return BadRequest("Email already exists.");

        // Simple Validation
        if (string.IsNullOrEmpty(newUser.Password)) return BadRequest("Password required.");

        if (newUser.City is not null && newUser.Region is not null)
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("User-Agent", "FindIT/1.0 (a_ramsden203976@fanshaweonline.ca)");

            string query = Uri.EscapeDataString($"{newUser.City}, {newUser.Region}");
            string url = $"https://nominatim.openstreetmap.org/search?q={query}&format=json&limit=1";

            try
            {
                var response = await client.GetFromJsonAsync<List<NominatimResponse>>(url);

                if (response != null && response.Count > 0)
                {
                    newUser.Latitude = double.Parse(response[0].Lat, System.Globalization.CultureInfo.InvariantCulture);
                    newUser.Longitude = double.Parse(response[0].Lon, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Geocoding failed: {ex.Message}");
            }
        }

        await _context.AddNewUser(newUser);

        return Ok("User creation successful!");
    }

    // POST: api/users/login : Login use Email and Password to check authentication password
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var user = await _context.GetUserByEmail(loginRequest.Email);
        if (user == null) return Unauthorized("Invalid Email");

        // Use BCrypt to verify the plain text password against the hashed one in DB
        bool isValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password);

        if (!isValid) return Unauthorized("Invalid Password");

        return Ok(new { id = user.Id, username = user.Username, email = user.Email });
    }

    // PUT: api/users/{username} : Updates an existing user by their username
    [HttpPut("{username}")]
    public async Task<IActionResult> Update(string username, [FromBody] User updatedUser)
    {
        var existingUser = await _context.GetUserByUsername(username);
        if (existingUser is null) return NotFound();

        if (updatedUser.City is not null && updatedUser.Region is not null && (updatedUser.City != existingUser.City || updatedUser.Region != existingUser.City))
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("User-Agent", "FindIT/1.0 (a_ramsden203976@fanshaweonline.ca)");

            string query = Uri.EscapeDataString($"{updatedUser.City}, {updatedUser.Region}");
            string url = $"https://nominatim.openstreetmap.org/search?q={query}&format=json&limit=1";

            try
            {
                var response = await client.GetFromJsonAsync<List<NominatimResponse>>(url);

                if (response != null && response.Count > 0)
                {
                    updatedUser.Latitude = double.Parse(response[0].Lat, System.Globalization.CultureInfo.InvariantCulture);
                    updatedUser.Longitude = double.Parse(response[0].Lon, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Geocoding failed: {ex.Message}");
            }
        }

        updatedUser.Id = existingUser.Id;
        updatedUser.Password = existingUser.Password;
        await _context.UpdateUserByUsername(username, updatedUser);
        return Ok("Update successful!");
    }

    // DELETE: api/users/{username} : Deletes a user by their username
    [HttpDelete("{username}")]
    public async Task<IActionResult> Delete(string username)
    {
        var user = await _context.GetUserByUsername(username);
        if (user == null) return NotFound();

        await _context.DeleteUserByUsername(username);
        return Ok("Delete successful!");
    }
}