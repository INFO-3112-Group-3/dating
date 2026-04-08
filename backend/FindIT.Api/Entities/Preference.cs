using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using FindIT.Api.Entities;
namespace FindIT.Api.Entities;

public class Preference
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string? Pref {get; set;}


}
