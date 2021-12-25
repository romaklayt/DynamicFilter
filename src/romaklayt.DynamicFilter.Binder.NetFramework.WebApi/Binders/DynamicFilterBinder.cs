using System;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Binders;

public class DynamicFilterBinder : DynamicComplexBinder
{
    public override bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
    {
        if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

        var model = Activator.CreateInstance(bindingContext.ModelType);

        ExtractFilters(model, bindingContext);

        bindingContext.Model = model;

        return true;
    }
}