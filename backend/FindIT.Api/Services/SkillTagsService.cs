using FindIT.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FindIT.Api.Services
{
    public class SkillTagsService
    {
        private readonly IMongoCollection<SkillTags> _skillTagsCollection;

        public SkillTagsService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
            _skillTagsCollection = mongoDatabase.GetCollection<SkillTags>(databaseSettings.Value.SkillTagsCollectionName);
        }

        // Get all skill tags
        public async Task<List<SkillTags>> GetAsync() => await _skillTagsCollection.Find(_ => true).ToListAsync();

        // Add a new skill tag
        public async Task CreateAsync(SkillTags newTag) => await _skillTagsCollection.InsertOneAsync(newTag);
    }
}
