using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder.Net
{
    [ModelBinder(BinderType = typeof(DynamicFilterBinder))]
    public class DynamicFilterModel : BaseDynamicFilter
    {
    }
}