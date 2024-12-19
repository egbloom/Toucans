using ToucansApi.Core.DTOs;

namespace ToucansApi.Core.Extensions;

public static class ResponseExtensions
{
    public static ApiResponse<T> ToApiResponse<T>(
        this T data,
        string? message = null)
    {
        return ApiResponse<T>.Ok(data, message);
    }

    public static ApiResponse<List<TDestination>> ToApiResponseList<TSource, TDestination>(
        this IEnumerable<TSource> source,
        Func<TSource, TDestination> mapper)
    {
        var mappedData = source.Select(mapper).ToList();
        return ApiResponse<List<TDestination>>.Ok(mappedData);
    }

    public static async Task<ApiResponse<PaginatedResponse<TDestination>>>
        ToPaginatedApiResponse<TSource, TDestination>(
            this IQueryable<TSource> query,
            Func<TSource, TDestination> mapper,
            BaseFilterDto parameters)
    {
        var paginatedData = await query.ToPaginatedResponseAsync(
            parameters.PageNumber,
            parameters.PageSize);

        var mappedItems = paginatedData.Items.Select(mapper);

        var response = new PaginatedResponse<TDestination>(
            mappedItems,
            paginatedData.TotalItems,
            paginatedData.CurrentPage,
            parameters.PageSize);

        return ApiResponse<PaginatedResponse<TDestination>>.Ok(response);
    }
}