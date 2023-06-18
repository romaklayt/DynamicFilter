using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Binder.Net.Models;

public class DynamicPagingModel : DynamicFilterModel, IDynamicPaging
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}