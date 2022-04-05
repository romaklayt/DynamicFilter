using System.Web.Http.ModelBinding;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Binders;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Models;

[ModelBinder(typeof(DynamicComplexBinder))]
public class DynamicComplexModel : IDynamicComplex
{
    public string Order { get; set; }
    public string Filter { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string Select { get; set; }
}