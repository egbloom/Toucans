using Microsoft.EntityFrameworkCore;

namespace ToucansApi.Functions.Extensions;

public static class QueryableExtensions
{
    public static async Task<PaginatedResponse<T>> ToPaginatedResponseAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        // Ensure valid pagination parameters
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, Math.Min(100, pageSize));

        // Get total count of items
        var totalItems = await query.CountAsync();

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasNextPage = pageNumber < totalPages,
            HasPreviousPage = pageNumber > 1
        };
    }
}

public class PaginatedResponse<T>
{
    public required IEnumerable<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}