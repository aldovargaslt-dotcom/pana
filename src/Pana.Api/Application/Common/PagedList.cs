namespace Pana.Api.Application.Common;

/// <summary>
/// Generic paginated list for GetAll endpoints.
/// </summary>
public record PagedList<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
