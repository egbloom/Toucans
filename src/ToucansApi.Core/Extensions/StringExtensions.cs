namespace ToucansApi.Core.Extensions;

public static class StringExtensions
{
    public static bool ContainsIgnoreCase(
        this string source,
        string search)
    {
        return source?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static string ToSlug(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace(".", "-")
            .Replace("/", "-")
            .Replace("\\", "-")
            .Replace(":", "-")
            .Replace("?", "")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("\"", "");
    }
}