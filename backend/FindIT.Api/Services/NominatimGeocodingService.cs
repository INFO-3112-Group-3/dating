using FindIT.Api.Models;

namespace FindIT.Api.Services
{
    /// <summary>
    /// Implementation of IGeocodingService using the OpenStreetMap Nominatim API.
    /// This service converts human-readable addresses (City, Region) into Geographic coordinates.
    /// </summary>
    public class NominatimGeocodingService : IGeocodingService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Constructor utilizing IHttpClientFactory to manage HttpClient lifecycles efficiently,
        /// preventing socket exhaustion issues.
        /// </summary>
        public NominatimGeocodingService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Asynchronously fetches Latitude and Longitude for a given location.
        /// </summary>
        /// <param name="city">The name of the city (e.g., "London").</param>
        /// <param name="region">The state or province (e.g., "Ontario").</param>
        /// <returns>
        /// A Tuple containing (Lat, Lon) if the search is successful; 
        /// otherwise, returns null if the location is not found or the API call fails.
        /// </returns>
        public async Task<(double Lat, double Lon)?> GetCoordinatesAsync(string city, string region)
        {
            // Create a client from the factory (recommended over 'new HttpClient()')
            var client = _httpClientFactory.CreateClient();

            // MANDATORY: Nominatim's Usage Policy requires a descriptive User-Agent.
            // Failure to provide this can lead to 403 Forbidden responses.
            client.DefaultRequestHeaders.Add("User-Agent", "FindIT/1.0 (Contact: support@findit-app.com)");

            // Sanitize input to ensure special characters or spaces don't break the URL.
            string query = Uri.EscapeDataString($"{city}, {region}");

            // limit=1 ensures we only get the most relevant result to save bandwidth.
            string url = $"https://nominatim.openstreetmap.org/search?q={query}&format=json&limit=1";

            try
            {
                // Nominatim returns a JSON array, even for a single result.
                var response = await client.GetFromJsonAsync<List<NominatimResponse>>(url);

                if (response != null && response.Any())
                {
                    // Nominatim returns coordinates as strings; we parse them to doubles for our internal logic.
                    return (
                        double.Parse(response[0].Lat, System.Globalization.CultureInfo.InvariantCulture),
                        double.Parse(response[0].Lon, System.Globalization.CultureInfo.InvariantCulture)
                    );
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Log specific network failures (Timeout, DNS issues, etc.)
                Console.WriteLine($"Geocoding Network Error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                // Catch-all for parsing errors or unexpected nulls
                Console.WriteLine($"Geocoding failed: {ex.Message}");
            }

            return null;
        }
    }
}