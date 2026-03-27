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

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _usersCollection.Find(x => x.Username == username).FirstOrDefaultAsync();

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

        if (updates.Nickname != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.Nickname, updates.Nickname));

        if (updates.FirstName != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.FirstName, updates.FirstName));

        if (updates.LastName != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.LastName, updates.LastName));

        if (updates.Gender.HasValue)
            updateDefinitions.Add(updateBuilder.Set(u => u.Gender, updates.Gender.Value));

        if (updates.Age.HasValue)
            updateDefinitions.Add(updateBuilder.Set(u => u.Age, updates.Age.Value));

        if (updates.PreferredContact.HasValue)
            updateDefinitions.Add(updateBuilder.Set(u => u.PreferredContact, updates.PreferredContact.Value));

        if (updates.ContactIdentifier != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.ContactIdentifier, updates.ContactIdentifier));

        if (updates.City != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.City, updates.City));

        if (updates.Region != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.Region, updates.Region));

        if (updates.Preferences != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.Preferences, updates.Preferences));

        if (newLocation != null)
            updateDefinitions.Add(updateBuilder.Set(u => u.Location, newLocation));

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

    /// <summary>
    /// Core matchmaking logic. Finds users who meet mutual preference criteria:
    /// 1. Mutual Age interest (User A likes User B's age AND User B likes User A's age).
    /// 2. Mutual Gender interest.
    /// 3. Physical proximity using MongoDB's $nearSphere.
    /// </summary>
    /// <param name="currentUser">The user for whom we are finding matches.</param>
    /// <returns>A list of up to 100 potential matches.</returns>
    public async Task<List<User>> GetPotentialMatchesAsync(User currentUser)
    {
        var builder = Builders<User>.Filter;
        var today = DateTime.Today;

        // --- 1. Calculate Date Ranges for the "Potential Match" ---
        // Someone is within currentUser's Age Prefs if their DOB is between these dates:
        var matchMinBirthDate = today.AddYears(-currentUser.Preferences.MaxAge - 1).AddDays(1);
        var matchMaxBirthDate = today.AddYears(-currentUser.Preferences.MinAge);

        // --- 2. Calculate Date Ranges for "Current User" (Mutual Match check) ---
        // For the match to want the CurrentUser, the CurrentUser's DOB must fit 
        // inside the match's Preference range.
        // We use currentUser.DateOfBirth as the anchor for the filters below.

        // Base Filter: Don't match with self
        var filters = builder.Ne(u => u.Id, currentUser.Id);

        // DOB Filter: Does the candidate fit the current user's age requirements?
        filters &= builder.Gte(u => u.DateOfBirth, matchMinBirthDate);
        filters &= builder.Lte(u => u.DateOfBirth, matchMaxBirthDate);

        // Mutual Age Filter: Does the current user fit the candidate's age requirements?
        // Logic: Candidate's MinAge must be <= CurrentUser.Age AND Candidate's MaxAge must be >= CurrentUser.Age
        filters &= builder.Lte(u => u.Preferences.MinAge, currentUser.Age);
        filters &= builder.Gte(u => u.Preferences.MaxAge, currentUser.Age);

        // Gender & Mutual Interest Filter
        if (currentUser.Preferences.InterestedIn?.Any() == true)
        {
            filters &= builder.In(u => u.Gender, currentUser.Preferences.InterestedIn);
        }
        filters &= builder.AnyEq(u => u.Preferences.InterestedIn, currentUser.Gender);

        // Distance Filter
        double distanceInMeters = currentUser.Preferences.MaxDistance * 1000;
        var point = MongoDB.Driver.GeoJsonObjectModel.GeoJson.Point(
            MongoDB.Driver.GeoJsonObjectModel.GeoJson.Geographic(
                currentUser.Location[0],
                currentUser.Location[1]
            )
        );

        filters &= builder.NearSphere(u => u.Location, point, maxDistance: distanceInMeters);

        return await _usersCollection.Find(filters).Limit(100).ToListAsync();
    }
}