using FindIT.Api.DTOs;
using FindIT.Api.Entities;
using FindIT.Api.Helpers;

namespace FindIT.Api.Services;

/// <summary>
/// Service responsible for ranking potential matches based on compatibility.
/// This acts as a secondary filter to sort results that have already passed 
/// the primary database filters (Age, Gender, Location).
/// </summary>
public class MatchingService
{
    /// <summary>
    /// Processes a list of candidate users and ranks them by compatibility score.
    /// </summary>
    /// <param name="currentUser">The user seeking a match.</param>
    /// <param name="databaseResults">The pre-filtered list of users from MongoDB.</param>
    /// <returns>A list of MatchScore objects sorted from highest to lowest score.</returns>
    public List<MatchScore> GetMatches(User currentUser, List<User> databaseResults)
    {
        return databaseResults
            .Select(candidate => new MatchScore
            {
                Profile = candidate.ToPublicDto()!,
                // Calculate compatibility for every user in the result set
                TotalScore = CalculateScore(currentUser, candidate)
            })
            // Sort so the most compatible users appear first in the UI
            .OrderByDescending(m => m.TotalScore)
            .ToList();
    }

    /// <summary>
    /// Internal logic for weighting compatibility. 
    /// Currently, interests are weighted more heavily (10pts) than skills (5pts).
    /// </summary>
    /// <param name="user">The active user.</param>
    /// <param name="candidate">The potential match being evaluated.</param>
    /// <returns>An integer representing the total compatibility points.</returns>
    private int CalculateScore(User user, User candidate)
    {
        int score = 0;

        // 1. Shared Interests: High-level compatibility.
        // Intersect finds common strings between both lists.
        // Weight: 10 points per shared interest.
        int sharedInterests = user.Interests.Intersect(candidate.Interests).Count();
        score += (sharedInterests * 10);

        // 2. Shared Skills: Professional/Functional compatibility.
        // Weight: 5 points per shared skill.
        int sharedSkills = user.Skills.Intersect(candidate.Skills).Count();
        score += (sharedSkills * 5);

        return score;
    }
}