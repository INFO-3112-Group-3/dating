namespace FindIT.Api.Models
{
    /// <summary>
    /// Used when updating the used data, excludes password/email/username.
    /// </summary>
    public class UpdatedUserProfile
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Orientation { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Occupation { get; set; }
        public string Notes { get; set; }

        public List<string> Skills { get; set; }
        public List<string> Interests { get; set; }
    }
}
