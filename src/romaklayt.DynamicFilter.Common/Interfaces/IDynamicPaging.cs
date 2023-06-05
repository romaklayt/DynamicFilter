namespace romaklayt.DynamicFilter.Common.Interfaces;

public interface IDynamicPaging : IDynamicFilter
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}