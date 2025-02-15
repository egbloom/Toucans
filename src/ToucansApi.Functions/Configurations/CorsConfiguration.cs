namespace ToucansApi.Functions.Configurations;

public static class CorsConfiguration
{
    public static string[] AllowedOrigins =>
    [
        "*",
        "http://localhost:3000",
        "https://your-production-frontend.com"
    ];

    public static string[] AllowedMethods =>
    [
        "GET",
        "POST",
        "PUT",
        "DELETE",
        "PATCH",
        "OPTIONS"
    ];
}