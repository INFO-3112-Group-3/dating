using FindIT.Api.DTOs;
using FindIT.Api.Entities;
using MongoDB.Driver;

namespace FindIT.Api.Services;

public class DashboardService
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IMongoCollection<MatchHistory> _matchHistoryCollection; 

    public DashboardService(IMongoDatabase database)
    {
        _usersCollection = database.GetCollection<User>("Users");
        _matchHistoryCollection = database.GetCollection<MatchHistory>("MatchHistory");
    }

    /// <summary>
    /// Retrieves key statistics for the management dashboard, including counts of free and paid members, total matches where communication was exposed, and total matches recorded to date.
    /// </summary>
    /// <returns>A <see cref="DashboardStatsDto"/> containing the dashboard statistics.</returns>
    public async Task<DashboardStatsDto> GetManagementStatsAsync()
    {
        return new DashboardStatsDto
        {
            // Count based on IsPaidUser flag in User entity
            FreeMembersCount = await _usersCollection.CountDocumentsAsync(u => !u.IsPaidUser),
            PaidMembersCount = await _usersCollection.CountDocumentsAsync(u => u.IsPaidUser),

            // Count matches where communication was exposed (assuming this means matches where contact info was shown, which is tracked by CommunicationExposed in MatchHistory)
            TotalExposedMatches = await _matchHistoryCollection.CountDocumentsAsync(m => m.CommunicationExposed),

            // Total matches recorded to date (assuming this means all match history entries)
            TotalMatchesToDate = await _matchHistoryCollection.CountDocumentsAsync(_ => true)
        };
    }
}