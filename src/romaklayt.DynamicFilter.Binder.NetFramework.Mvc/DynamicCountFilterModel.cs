using System.Web.Mvc;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder.NetFramework.Mvc
{
    [ModelBinder(typeof(DynamicCountFilterBinder))]
    public class DynamicCountFilterModel : BaseCountDynamicFilter
    {
    }
}