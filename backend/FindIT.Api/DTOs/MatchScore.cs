namespace FindIT.Api.DTOs
{
    public class MatchScore
    {
        public UserPublicDto Profile { get; set; } = null!;
        public int TotalScore { get; set; }
    }
}