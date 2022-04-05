using System.Web.Mvc;
using romaklayt.DynamicFilter.Binder.NetFramework.Mvc.Binders;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Binder.NetFramework.Mvc.Models;

[ModelBinder(typeof(DynamicComplexBinder))]
public class DynamicComplexModel : IDynamicComplex
{
    public string Order { get; set; }
    public string Filter { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string Select { get; set; }
}