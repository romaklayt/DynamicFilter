using System.Web.Mvc;
using romaklayt.DynamicFilter.Binder.NetFramework.Mvc.Binders;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Binder.NetFramework.Mvc.Models;

[ModelBinder(typeof(DynamicComplexBinder))]
public class DynamicSelectModel : IDynamicSelect
{
    public string Select { get; set; }
}