namespace ToucansApi.Core.DTOs;

public class BaseFilterDto
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string? SortBy { get; set; }
    public bool IsDescending { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}