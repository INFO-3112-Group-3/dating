using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using FindIT.Api.Database;
using FindIT.Api.Models;

namespace FindIT.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
	private readonly MongoContext _context;

	public UsersController(MongoContext context)
	{
		_context = context;
	}

	// GET: api/users
	[HttpGet]
	public async Task<List<User>> GetAll()
	{
		var collection = _context.GetCollection<User>("Users");
		return await collection.Find(_ => true).ToListAsync();
	}

	// POST: api/users
	[HttpPost]
	public async Task<ActionResult<User>> Create([FromBody] User newUser)
	{
		if (string.IsNullOrWhiteSpace(newUser.Username) ||
			string.IsNullOrWhiteSpace(newUser.Email) ||
			string.IsNullOrWhiteSpace(newUser.Password))
		{
			return BadRequest("Username, Email, and Password are required.");
		}

		var collection = _context.GetCollection<User>("UsersCollection");
		await collection.InsertOneAsync(newUser);

		return Ok(newUser);
	}
}