using System;
using System.Web.Mvc;

namespace romaklayt.DynamicFilter.Binder.NetFramework.Mvc.Binders
{
    public class DynamicFilterBinder : DynamicComplexBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var model = Activator.CreateInstance(bindingContext.ModelType);

            ExtractFilters(model, bindingContext);

            return model;
        }
    }
}