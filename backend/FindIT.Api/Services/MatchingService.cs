using FindIT.Api.DTOs;
using FindIT.Api.Entities;
using FindIT.Api.Helpers;
using FindIT.Api.Models;
using FluentAssertions;
using MongoDB.Driver;
using System.Collections.Generic;
namespace FindIT.Api.Services;

/// <summary>
/// Service responsible for ranking potential matches based on compatibility.
/// This acts as a secondary filter to sort results that have already passed 
/// the primary database filters (Age, Gender, Location).
/// </summary>
public class MatchingService
{
    //so uhh don't mind this... this is cursed as hell BUT yes.. this does store functions in a dictonary
    private Dictionary<string,Func<User,string,int,int>> preferenceFunctions = new Dictionary<string, Func<User,string,int,int>>();

    // MongoDB collections for logging match history -> this is for the dashboard and analytics, not for the matching process itself
    private readonly IMongoCollection<MatchHistory> _matchHistoryCollection;

    //THIS is where you will put the private functions for the different criteria checks
    private int GenderPref(User user,string preferenceInfo, int importance)
    {
        if (user.Gender.ToString() == preferenceInfo)
        {
            return importance;
        }
        else
        {
            return (0-importance);
        }
    }
    private int AboveAgePref(User user,string preferenceInfo, int importance)
    {
        if (user.Age < int.Parse(preferenceInfo))
        {
            return importance;
        }
        else
        {
            return (0-importance);
        }
    }
    private int BelowAgePref(User user,string preferenceInfo, int importance)
    {
        if (user.Age > int.Parse(preferenceInfo))
        {
            return importance;
        }
        else
        {
            return (0-importance);
        }
    }
    //constructor
    //when you create a new preference method, add it here with the coresponding preference "tag" for the key
    public MatchingService(IMongoDatabase database)
    {
        // Initialize the collection based on your DatabaseSettings
        _matchHistoryCollection = database.GetCollection<MatchHistory>("MatchHistory");

        preferenceFunctions.Add("Gender", GenderPref);
        preferenceFunctions.Add("AboveAge",AboveAgePref);
        preferenceFunctions.Add("BelowAge",BelowAgePref);
    }


    /// <summary>
    /// Processes a list of candidate users and ranks them by compatibility score.
    /// Logging the matches to the database for dashboard analytics. This allows us to track which matches are being generated and how users are interacting with them, providing valuable insights for improving our matching algorithms and user experience over time.
    /// </summary>
    /// <param name="currentUser">The user seeking a match.</param>
    /// <param name="databaseResults">The pre-filtered list of users from MongoDB.</param>
    /// <returns>A list of MatchScore objects sorted from highest to lowest score.</returns>
    /// *** change method to be async, Task method that also logs the matches to the database for the dashboard analytics
    public async Task<List<MatchScore>> GetMatches(User currentUser,List<User> databaseResults)
    {
        List<MatchScore> matches = databaseResults
            .Select(candidate => new MatchScore
            {
                Profile = candidate.ToPublicDto()!,
                // Calculate compatibility for every user in the result set
                TotalScore = CalculateScore(currentUser, candidate)
            })
            // Sort so the most compatible users appear first in the UI
            .OrderByDescending(m => m.TotalScore)
            .ToList();

            //can change to to finer tweak scores
            matches.RemoveAll(x => x.TotalScore < 3);

        // This is for dashboard analytics. Persist these matches to the database for the Dashboard
        foreach (var match in matches)
        {
            var history = new MatchHistory
            {
                UserAId = currentUser.Id!,
                UserBId = match.Profile.Id!,
                MatchScore = match.TotalScore,
                CommunicationExposed = currentUser.IsPaidUser,
                CreatedAt = DateTime.UtcNow
            };
            await _matchHistoryCollection.InsertOneAsync(history);
        }

        return matches;
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
        // eidting 
        int score = 0;

        //running through the users criteria
        foreach (UserPreference pref in user.Preferences)
        {
            if(preferenceFunctions.ContainsKey(pref.PreferenceType!))
            {
                score += preferenceFunctions[pref.PreferenceType!](candidate,pref.PreferenceInfo!,pref.Importance);
            }
        }

        return score;
    }
}