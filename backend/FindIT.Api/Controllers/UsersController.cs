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

    // GET: api/users
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.GetAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _context.GetAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User newUser)
    {
        if (string.IsNullOrWhiteSpace(newUser.Username) ||
            string.IsNullOrWhiteSpace(newUser.Email) ||
            string.IsNullOrWhiteSpace(newUser.Password))
        {
            return BadRequest("Username, Email, and Password are required.");
        }

        await _context.CreateAsync(newUser);
        return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] User updatedUser)
    {
        var existingUser = await _context.GetAsync(id);
        if (existingUser is null)
        {
            return NotFound();
        }
        updatedUser.Id = existingUser.Id; // Ensure the ID remains unchanged
        await _context.UpdateAsync(id, updatedUser);
        return NoContent();
    }
}