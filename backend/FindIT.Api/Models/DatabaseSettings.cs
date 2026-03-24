namespace FindIT.Api.Models
{
    // sed to store the appsettings.json file's BookStoreDatabase property values.
    // The JSON and C# property names are named identically to ease the mapping process.
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string UsersCollectionName { get; set; } = null!;

        public string PaymentCollectionName { get; set; } = null!;

        public string SubscriptionsCollectionName { get; set; } = null!;

        public string SkillTagsCollectionName { get; set; } = null!;
    }
}
