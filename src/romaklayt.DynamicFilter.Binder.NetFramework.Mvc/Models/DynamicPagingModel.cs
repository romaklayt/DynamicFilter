using System.Web.Mvc;
using romaklayt.DynamicFilter.Binder.NetFramework.Mvc.Binders;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Binder.NetFramework.Mvc.Models;

[ModelBinder(typeof(DynamicComplexBinder))]
public class DynamicPagingModel : DynamicFilterModel, IDynamicPaging
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}