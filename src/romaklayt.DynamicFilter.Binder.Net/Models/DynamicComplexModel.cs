using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Binder.Net.Binders;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Binder.Net.Models;

[ModelBinder(BinderType = typeof(DynamicComplexBinder))]
public class DynamicComplexModel : DynamicPagingModel, IDynamicComplex
{
    public string Select { get; set; }
}