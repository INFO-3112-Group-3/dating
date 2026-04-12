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

        public async Task<List<UserMatch>> GetMatchesByStatusAsync(string userId, MatchStatus status)
        {
            return await _matchesCollection
                .Find(m => (m.RequesterId == userId || m.TargetId == userId) && m.Status == status)
                .ToListAsync();
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
            // Filter for the record where these two users are involved, regardless of who started it
            var filter = Builders<UserMatch>.Filter.Or(
                Builders<UserMatch>.Filter.And(
                    Builders<UserMatch>.Filter.Eq(m => m.RequesterId, requesterId),
                    Builders<UserMatch>.Filter.Eq(m => m.TargetId, targetId)
                ),
                Builders<UserMatch>.Filter.And(
                    Builders<UserMatch>.Filter.Eq(m => m.RequesterId, targetId),
                    Builders<UserMatch>.Filter.Eq(m => m.TargetId, requesterId)
                )
            );

            var update = Builders<UserMatch>.Update.Set(m => m.Rating, rating);
            await _matchesCollection.UpdateOneAsync(filter, update);
        }

        public async Task<List<string>> GetInteractedUserIdsAsync(string userId)
        {
            // Find all matches where the user is EITHER the requester or the target
            var matches = await _matchesCollection
                .Find(m => m.RequesterId == userId || m.TargetId == userId)
                .ToListAsync();

            // Collect IDs of the "other" person in all these interactions
            var interactedIds = matches.Select(m =>
                m.RequesterId == userId ? m.TargetId : m.RequesterId
            ).Distinct().ToList();

            return interactedIds;
        }
    }
}
