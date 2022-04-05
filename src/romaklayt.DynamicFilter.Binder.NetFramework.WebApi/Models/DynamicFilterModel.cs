using System.Web.Http.ModelBinding;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Binders;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Models;

[ModelBinder(typeof(DynamicComplexBinder))]
public class DynamicFilterModel : IDynamicFilter
{
    public string Order { get; set; }
    public string Filter { get; set; }
}