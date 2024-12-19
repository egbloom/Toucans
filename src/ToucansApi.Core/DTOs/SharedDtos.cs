namespace ToucansApi.Core.DTOs;

public class PaginatedResponse<T>
{
    public PaginatedResponse(IEnumerable<T> items, int totalItems, int currentPage, int pageSize)
    {
        Items = new List<T>(items);
        TotalItems = totalItems;
        CurrentPage = currentPage; // Updated parameter name
        PageSize = pageSize;

        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        HasNextPage = CurrentPage < TotalPages;
        HasPreviousPage = CurrentPage > 1;
    }

    public List<T> Items { get; }
    public int CurrentPage { get; } // Changed from PageNumber to CurrentPage
    public int PageSize { get; }
    public int TotalItems { get; }
    public int TotalPages { get; }
    public bool HasNextPage { get; }
    public bool HasPreviousPage { get; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T> { Success = true, Data = data, Message = message };
    }

    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T> { Success = false, Message = message };
    }
}