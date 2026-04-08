using System;
using FindIT.Api.Entities;
using Microsoft.VisualBasic;
using MongoDB.Driver;

namespace FindIT.Api.Services;

public class PreferenceService
{
    private readonly IMongoCollection<Preference> _preferenceCollection;

    public PreferenceService(IMongoDatabase database)
    {
        _preferenceCollection = database.GetCollection<Preference>("Preferences");
    }

    public async Task<List<string>> GetAsync()
    {
        var prefs = await _preferenceCollection.Find(_ => true).ToListAsync();
        List<string> prefsString = new List<string>();
        foreach(Preference pref in prefs)
        {
            prefsString.Add(pref.Pref);
        }

        return prefsString;
    }

    public async Task CreateAsync(Preference pref) =>
        await _preferenceCollection.InsertOneAsync(pref);
}
