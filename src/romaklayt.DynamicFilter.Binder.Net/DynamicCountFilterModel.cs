using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder.Net
{
    [ModelBinder(BinderType = typeof(DynamicCountFilterBinder))]
    public class DynamicCountFilterModel : BaseCountDynamicFilter
    {
    }
}