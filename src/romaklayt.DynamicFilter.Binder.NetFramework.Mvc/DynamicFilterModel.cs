using System.Web.Mvc;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder.NetFramework.Mvc
{
    [ModelBinder(typeof(DynamicFilterBinder))]
    public class DynamicFilterModel : BaseDynamicFilter
    {
    }
}