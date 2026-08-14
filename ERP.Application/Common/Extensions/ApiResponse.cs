using ERP.Application.Common.Base;

namespace ERP.Application.Common.Extensions;

/// <summary>
/// Standardized API response wrapper for consistent response format.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static ApiResponse<T> Fail(string error, string? correlationId = null) => new()
    {
        Success = false,
        Error = error,
        CorrelationId = correlationId
    };
}

/// <summary>
/// Paginated response wrapper
/// </summary>
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

/// <summary>
/// Pagination validation helper
/// </summary>
public static class PaginationValidator
{
    public const int MinPage = 1;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 20;

    public static (int Page, int PageSize) Normalize(int? page, int? pageSize)
    {
        var normalizedPage = page.HasValue && page.Value >= MinPage ? page.Value : MinPage;
        var normalizedPageSize = pageSize.HasValue
            ? Math.Clamp(pageSize.Value, MinPageSize, MaxPageSize)
            : DefaultPageSize;

        return (normalizedPage, normalizedPageSize);
    }
}
