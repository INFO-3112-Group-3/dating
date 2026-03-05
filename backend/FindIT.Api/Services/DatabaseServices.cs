using FindIT.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FindIT.Api.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UsersService(
            IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<User>(
                databaseSettings.Value.UsersCollectionName);
        }

        public async Task<List<User>> GetAll() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        public async Task<User?> GetUserByUsername(string username) =>
            await _usersCollection.Find(x => x.Username == username).FirstOrDefaultAsync();

        public async Task<User?> GetUserByEmail(string email) =>
            await _usersCollection.Find(x => x.Email == email).FirstOrDefaultAsync();

        public async Task AddNewUser(User newUser)
        {
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
            await _usersCollection.InsertOneAsync(newUser);
        }

        public async Task UpdateUserByUsername(string username, User updatedUser)
        {
            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                updatedUser.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);
            }
            await _usersCollection.ReplaceOneAsync(x => x.Username == username, updatedUser);
        }

        public async Task UpdateUser(string id, User updatedUser)
        {
            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                updatedUser.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);
            }
            await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);
        }

        public async Task DeleteUserByUsername(string username) =>
            await _usersCollection.DeleteOneAsync(x => x.Username == username);

        public async Task DeleteUserByEmail(string email) =>
            await _usersCollection.DeleteOneAsync(x => x.Email == email);


    }
}
