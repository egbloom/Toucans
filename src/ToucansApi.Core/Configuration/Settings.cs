namespace ToucansApi.Core.Configuration
{
    public class Settings
    {
        public int DefaultPageSize { get; set; }
        public int MaxPageSize { get; set; }
        public bool EnableSwagger { get; set; }
        public required string ApiUrl { get; set; }
        public required string Environment { get; set; }
        public int TimeoutInSeconds { get; set; }
    }

    public class AuthenticationConfiguration
    {
        public required string Authority { get; set; }
        public required string Audience { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public required string[] ValidIssuers { get; set; }
    }
}