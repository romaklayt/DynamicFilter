using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder
{
    [ModelBinder(BinderType = typeof(DynamicFilterBinder))]
    public class DynamicFilterModel : BaseDynamicFilter
    {
        public string Filter { get; set; }
        public string Order { get; set; }
        public string Select { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}