namespace FindIT.Api.DTOs
{
    // This DTO is used for sending dashboard statistics to the client. It includes counts of free and paid members, counts of exposed matches, and total matches to actual date.
    public class DashboardStatsDto
    {
        public long FreeMembersCount { get; set; }
        public long PaidMembersCount { get; set; }
        public long TotalExposedMatches { get; set; }
        public long TotalMatchesToDate { get; set; }
    }
}
