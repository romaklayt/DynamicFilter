using System.Web.Http.ModelBinding;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi
{
    [ModelBinder(typeof(DynamicFilterBinder))]
    public class DynamicFilterModel : BaseDynamicFilter
    {
        public string Filter { get; set; }
        public string Order { get; set; }
        public string Select { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}