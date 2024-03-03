using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Common.Models;

public class DynamicFilterModel : IDynamicFilter
{
    public string Order { get; set; }
    public string Filter { get; set; }
}