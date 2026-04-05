using FindIT.Api.Entities;
using MongoDB.Driver;

namespace FindIT.Api.Services;

public class SkillTagsService
{
    private readonly IMongoCollection<SkillTags> _skillTagsCollection;

    public SkillTagsService(IMongoDatabase database)
    {
        // Use the collection name string directly since we registered the DB in Program.cs
        _skillTagsCollection = database.GetCollection<SkillTags>("SkillTags");
    }

    // Get everything
    public async Task<List<SkillTags>> GetAsync() =>
        await _skillTagsCollection.Find(_ => true).ToListAsync();

    // Get by category (useful for UI tabs like "Programming", "Design", etc.)
    public async Task<List<SkillTags>> GetByCategoryAsync(string category) =>
        await _skillTagsCollection.Find(x => x.Category == category).ToListAsync();

    // Add a new tag
    public async Task CreateAsync(SkillTags newTag) =>
        await _skillTagsCollection.InsertOneAsync(newTag);

    // Bulk create (handy for seeding your database the first time)
    public async Task CreateMultipleAsync(List<SkillTags> tags) =>
        await _skillTagsCollection.InsertManyAsync(tags);

    public async Task DeleteAllAsync()
    {
      await _skillTagsCollection.DeleteManyAsync(_ => true);
    }
}