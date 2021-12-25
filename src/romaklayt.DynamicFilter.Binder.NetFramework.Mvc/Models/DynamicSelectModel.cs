using System.Web.Mvc;
using romaklayt.DynamicFilter.Binder.NetFramework.Mvc.Binders;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder.NetFramework.Mvc.Models;

[ModelBinder(typeof(DynamicSelectBinder))]
public class DynamicSelectModel : BaseDynamicSelectModel
{
}