namespace FindIT.Api.DTOs
{
    // This DTO represents a user's match score with another profile. It includes the public profile of the matched user and the total compatibility score.
    public class MatchScore
    {
        public UserPublicDto Profile { get; set; } = null!;
        public int TotalScore { get; set; }
    }
}