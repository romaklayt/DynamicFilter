namespace romaklayt.DynamicFilter.Common;

public static class PageInfoHeaders
{
    public const string CurrentPage = $"X-{nameof(CurrentPage)}";
    public const string TotalPages = $"X-{nameof(TotalPages)}";
    public const string PageSize = $"X-{nameof(PageSize)}";
    public const string TotalCount = $"X-{nameof(TotalCount)}";
    public const string HasPrevious = $"X-{nameof(HasPrevious)}";
    public const string HasNext = $"X-{nameof(HasNext)}";
}