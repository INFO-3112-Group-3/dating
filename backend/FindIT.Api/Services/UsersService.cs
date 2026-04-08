using FindIT.Api.DTOs;
using FindIT.Api.Entities;
using MongoDB.Driver;

namespace FindIT.Api.Services;

/// <summary>
/// Service responsible for managing User data in MongoDB, including profile management,
/// security (hashing), and complex matching logic using Geospatial indexing.
/// </summary>
public class UsersService
{
    private readonly IMongoCollection<User> _usersCollection;

    /// <summary>
    /// Initializes the service and ensures that a Geospatial index exists on the 'Location' field.
    /// This index is required for the $nearSphere queries used in matchmaking.
    /// </summary>
    /// <param name="database">The MongoDB database instance injected via Dependency Injection.</param>
    public virtual void SetupIndexes() // Logic moved to a method for clarity
    {
        // 2DSphere index supports queries that calculate distance on an earth-like sphere.
        var keys = Builders<User>.IndexKeys.Geo2DSphere(u => u.Location);
        var indexModel = new CreateIndexModel<User>(keys);
        _usersCollection.Indexes.CreateOne(indexModel);
    }

    public UsersService(IMongoDatabase database)
    {
        _usersCollection = database.GetCollection<User>("Users");

        // Ensure the Geospatial Index exists for matching immediately upon service creation
        var keys = Builders<User>.IndexKeys.Geo2DSphere(u => u.Location);
        var indexModel = new CreateIndexModel<User>(keys);
        _usersCollection.Indexes.CreateOne(indexModel);
    }

    public async Task<List<User>> GetAllAsync() =>
        await _usersCollection.Find(_ => true).ToListAsync();

    public async Task<User?> GetByIdAsync(string id) =>
        await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetByEmailAsync(string email) =>
        await _usersCollection.Find(x => x.Email == email).FirstOrDefaultAsync();

    /// <summary>
    /// Persists a new user to the database.
    /// Note: The raw password is never stored; it is hashed using BCrypt before insertion.
    /// </summary>
    public async Task CreateAsync(User newUser, string rawPassword)
    {
        newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);
        await _usersCollection.InsertOneAsync(newUser);
    }

    /// <summary>
    /// Updates user profile fields dynamically. This method uses a 'Patch' approach,
    /// only updating fields that are provided in the DTO to avoid overwriting existing data with nulls.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="updates">DTO containing potential updates.</param>
    /// <param name="newLocation">Optional coordinates [longitude, latitude] if geocoding was updated.</param>
    public async Task UpdateProfileAsync(string id, UpdateUserDto updates, double[]? newLocation = null)
    {
        var updateBuilder = Builders<User>.Update;
        var updateDefinitions = new List<UpdateDefinition<User>>();

        // Only add fields to the update command if they are explicitly provided in the request
        if (updates.Salutation != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.Salutation, updates.Salutation));

        if (updates.FirstName != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.FirstName, updates.FirstName));

        if (updates.Nickname != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.Nickname, updates.Nickname));

        if (updates.LastName != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.LastName, updates.LastName));

        if (updates.Gender.HasValue)
            updateDefinitions.Add(updateBuilder.Set(u => u.Gender, updates.Gender.Value));

        if (updates.ContactMethod != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.ContactMethod, updates.ContactMethod));

        if (updates.ContactInfo != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.ContactInfo, updates.ContactInfo));

        if (updates.City != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.City, updates.City));

        if (updates.Region != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.Region, updates.Region));

        if (updates.Preferences != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.Preferences, updates.Preferences));

        if (newLocation != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.Location, newLocation));

        if (updates.Skills != null && updates.Skills.Count > 0)
            updateDefinitions.Add(updateBuilder.Set(u => u.Skills, updates.Skills));

        if (updates.Interests != null && updates.Interests.Count > 0)
            updateDefinitions.Add(updateBuilder.Set(u => u.Interests, updates.Interests));

        if (updates.Bio != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.Bio, updates.Bio));

        if (updates.ProfilePictureBase64 != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.ProfilePictureBase64, updates.ProfilePictureBase64));

        if (updates.IsAdminUser != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.IsAdminUser, updates.IsAdminUser));

        if (updates.IsPaidUser != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.IsPaidUser, updates.IsPaidUser));

        // Prevent unnecessary database round-trips if no changes were detected
        if (updateDefinitions.Count == 0) return;

        // Combine all 'Set' operations into a single atomic update for efficiency and consistency
        var combinedUpdate = updateBuilder.Combine(updateDefinitions);
        await _usersCollection.UpdateOneAsync(u => u.Id == id, combinedUpdate);
    }

    /// <summary>
    /// Re-hashes and updates a user's password.
    /// </summary>
    public async Task UpdatePasswordAsync(string id, string newRawPassword)
    {
        var newHash = BCrypt.Net.BCrypt.HashPassword(newRawPassword);
        var update = Builders<User>.Update.Set(u => u.PasswordHash, newHash);
        await _usersCollection.UpdateOneAsync(u => u.Id == id, update);
    }

    public async Task DeleteAsync(string id) =>
        await _usersCollection.DeleteOneAsync(x => x.Id == id);

    /// <summary>
    /// Adds skills to a user's profile using $addToSetEach.
    /// This ensures no duplicate skills are added even if the method is called multiple times.
    /// </summary>
    public async Task AddSkillsAsync(string id, List<string> skills)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        var update = Builders<User>.Update.AddToSetEach(u => u.Skills, skills);
        await _usersCollection.UpdateOneAsync(filter, update);
    }

    /// <summary>
    /// Adds interests to a user's profile using an atomic $addToSet operation.
    /// </summary>
    public async Task AddInterestsAsync(string id, List<string> interests)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        var update = Builders<User>.Update.AddToSetEach(u => u.Interests, interests);
        await _usersCollection.UpdateOneAsync(filter, update);
    }


}