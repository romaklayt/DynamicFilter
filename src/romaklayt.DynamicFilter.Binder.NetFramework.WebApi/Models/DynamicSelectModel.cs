using System.Web.Http.ModelBinding;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Binders;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Models
{
    [ModelBinder(typeof(DynamicSelectBinder))]
    public class DynamicSelectModel : BaseDynamicSelectModel
    {
    }
}