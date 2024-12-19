using ToucansApi.Core.DTOs;

namespace ToucansApi.Functions.Extensions
{
    public static class ApiResponseExtensions
    {
        public static ApiResponse<IEnumerable<T>> ToApiResponse<T>(this IEnumerable<T> items)
        {
            return new ApiResponse<IEnumerable<T>>
            {
                Success = true,
                Data = items,
                Message = null
            };
        }

        public static ApiResponse<T> ToApiResponse<T>(this T item)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = item,
                Message = null
            };
        }

        public static ApiResponse<PaginatedResponse<T>> ToApiResponse<T>(this PaginatedResponse<T> paginatedItems)
        {
            return new ApiResponse<PaginatedResponse<T>>
            {
                Success = true,
                Data = paginatedItems,
                Message = null
            };
        }
    }
}