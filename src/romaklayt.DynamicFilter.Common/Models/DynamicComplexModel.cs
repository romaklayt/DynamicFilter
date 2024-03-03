using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Common.Models;

public class DynamicComplexModel : DynamicPagingModel, IDynamicComplex
{
    public DynamicComplexModel()
    {
    }

    public DynamicComplexModel(params string[] members) => Select = string.Join(",", members);

    public string Select { get; set; }
}