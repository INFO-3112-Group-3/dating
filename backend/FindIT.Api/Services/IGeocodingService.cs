namespace FindIT.Api.Services
{
    public interface IGeocodingService
    {
        Task<(double Lat, double Lon)?> GetCoordinatesAsync(string city, string region);
    }
}
