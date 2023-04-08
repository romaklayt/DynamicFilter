namespace romaklayt.DynamicFilter.Common.Interfaces;

public interface IPageInfo
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious { get; }
    public bool HasNext { get; }
}