namespace ToucansApi.Functions.Configurations
{
    public static class CorsConfiguration
    {
        public static string[] AllowedOrigins => new[]
        {
            "*",
            "http://localhost:3000",
            "https://your-production-frontend.com"
        };

        public static string[] AllowedMethods => new[]
        {
            "GET",
            "POST",
            "PUT",
            "DELETE",
            "PATCH",
            "OPTIONS"
        };
    }
}