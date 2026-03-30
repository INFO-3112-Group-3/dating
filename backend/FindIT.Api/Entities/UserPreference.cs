using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FindIT.Api.Entities;

public class UserPreference
{
    public string? PreferenceType {get;set;}
    public string? PreferenceInfo {get; set;}
    public int Importance {get;set;}
    
}