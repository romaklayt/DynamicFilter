using System.Web.Http.ModelBinding;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi
{
    [ModelBinder(typeof(DynamicCounterFilterBinder))]
    public class DynamicCountFilterModel : BaseCountDynamicFilter
    {
    }
}