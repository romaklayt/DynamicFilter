using System.Web.Http.ModelBinding;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi
{
    [ModelBinder(typeof(DynamicFilterBinder))]
    public class DynamicFilterModel : BaseDynamicFilter
    {
    }
}