using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Binder.Net.Binders;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Binder.Net.Models;

[ModelBinder(BinderType = typeof(DynamicComplexBinder))]
public class DynamicComplexModel : IDynamicComplex
{
    public string Order { get; set; }
    public string Filter { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string Select { get; set; }
}