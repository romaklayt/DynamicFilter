using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Binder.Net.Binders;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder.Net.Models
{
    [ModelBinder(BinderType = typeof(DynamicFilterBinder))]
    public class DynamicFilterModel : BaseDynamicFilterModel
    {
    }
}