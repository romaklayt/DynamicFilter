using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Common.Models;

public class DynamicSelectModel : IDynamicSelect
{
    public DynamicSelectModel()
    {
    }

    public DynamicSelectModel(params string[] members) => Select = string.Join(",", members);

    public string Select { get; set; }
}