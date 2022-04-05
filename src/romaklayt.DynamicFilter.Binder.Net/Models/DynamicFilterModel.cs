using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Binder.Net.Binders;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Binder.Net.Models;

[ModelBinder(BinderType = typeof(DynamicComplexBinder))]
public class DynamicFilterModel : IDynamicFilter
{
    public string Order { get; set; }
    public string Filter { get; set; }
}