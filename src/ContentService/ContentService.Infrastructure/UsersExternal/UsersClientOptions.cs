namespace ContentService.Infrastructure.UsersExternal
{
    public sealed class UsersClientOptions
    {
        public string BaseUrl { get; set; } = "https://localhost:7146";
        public ServiceAccountOptions ServiceAccount { get; set; } = new();
        public sealed class ServiceAccountOptions
        {
            public string Email { get; set; } = "admin@cms.local";
            public string Password { get; set; } = "P@ssw0rd!";
        }
    }
}
