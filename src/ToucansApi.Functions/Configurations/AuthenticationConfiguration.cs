namespace ToucansApi.Functions.Configurations
{
    public class AuthenticationConfiguration
    {
        public required string Authority { get; set; }
        public required string Audience { get; set; }
        public bool RequireHttpsMetadata { get; set; }
    }
}