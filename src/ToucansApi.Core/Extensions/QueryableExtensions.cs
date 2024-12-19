using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ToucansApi.Core.DTOs;

namespace ToucansApi.Core.Extensions;

public static class QueryableExtensions
{
    public static async Task<PaginatedResponse<T>> ToPaginatedResponseAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<T>(items, totalItems, pageNumber, pageSize);
    }

    public static IQueryable<T> ApplyFilter<T>(
        this IQueryable<T> query,
        BaseFilterDto filter)
    {
        if (!string.IsNullOrEmpty(filter.SortBy))
        {
            var property = typeof(T).GetProperty(filter.SortBy);
            if (property != null)
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var propertyAccess = Expression.Property(parameter, property);
                var lambda = Expression.Lambda<Func<T, object>>(
                    Expression.Convert(propertyAccess, typeof(object)),
                    parameter);

                query = filter.IsDescending
                    ? query.OrderByDescending(lambda)
                    : query.OrderBy(lambda);
            }
        }

        return query;
    }
}