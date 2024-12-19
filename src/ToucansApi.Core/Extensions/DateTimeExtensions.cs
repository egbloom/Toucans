namespace ToucansApi.Core.Extensions;

public static class DateTimeExtensions
{
    public static bool IsBetween(
        this DateTime date,
        DateTime? start,
        DateTime? end)
    {
        if (!start.HasValue && !end.HasValue)
            return true;

        if (start.HasValue && !end.HasValue)
            return date >= start.Value;

        if (!start.HasValue && end.HasValue)
            return date <= end.Value;

        return date >= start.Value && date <= end.Value;
    }

    public static DateTime StartOfDay(this DateTime date)
    {
        return date.Date;
    }

    public static DateTime EndOfDay(this DateTime date)
    {
        return date.Date.AddDays(1).AddTicks(-1);
    }
}