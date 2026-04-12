using FindIT.Api.Entities;
using MongoDB.Driver;

namespace FindIT.Api.Services
{
    public class MatchesService
    {
        private readonly IMongoCollection<UserMatch> _matchesCollection;

        public MatchesService(IMongoDatabase database)
        {
            _matchesCollection = database.GetCollection<UserMatch>("Matches");
        }


        public async Task<UserMatch?> GetMatchAsync(string requesterId, string targetId)
        {
            return await _matchesCollection
                .Find(m => m.RequesterId == requesterId && m.TargetId == targetId)
                .FirstOrDefaultAsync();
        }

        public async Task CreateMatchAsync(string requesterId, string targetId, MatchStatus status)
        {
            var match = new UserMatch
            {
                RequesterId = requesterId,
                TargetId = targetId,
                Status = status
            };
            await _matchesCollection.InsertOneAsync(match);
        }

        public async Task UpdateStatusAsync(string matchId, MatchStatus status)
        {
            var update = Builders<UserMatch>.Update.Set(m => m.Status, status);
            await _matchesCollection.UpdateOneAsync(m => m.Id == matchId, update);
        }


        public async Task SetRatingAsync(string requesterId, string targetId, int rating)
        {
            var filter = Builders<UserMatch>.Filter.And(
                Builders<UserMatch>.Filter.Eq(m => m.RequesterId, requesterId),
                Builders<UserMatch>.Filter.Eq(m => m.TargetId, targetId)
            );
            var update = Builders<UserMatch>.Update.Set(m => m.Rating, rating);
            await _matchesCollection.UpdateOneAsync(filter, update);
        }
    }
}
