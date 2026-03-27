using System.Text.Json.Serialization;

namespace FindIT.Api.Models
{
    public class NominatimResponse
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; }
        [JsonPropertyName("lon")]
        public string Lon { get; set; }
    }
}
