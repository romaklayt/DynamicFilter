using System;
using System.Web.Mvc;

namespace romaklayt.DynamicFilter.Binder.NetFramework.Mvc
{
    public class DynamicFilterBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var model = Activator.CreateInstance(bindingContext.ModelType);

            ExtractFilters(model, bindingContext);

            ExtractOrder(model, bindingContext);

            ExtractPagination(model, bindingContext);

            ExtractSelect(model, bindingContext);
            
            return model;
        }

        private static void ExtractPagination(object model, ModelBindingContext bindingContext)
        {
            var page = bindingContext.ValueProvider.GetValue("page")?.AttemptedValue;
            var pageSize = bindingContext.ValueProvider.GetValue("pagesize")?.AttemptedValue;

            if (!string.IsNullOrWhiteSpace(page))
                model.GetType().GetProperty("Page")?.SetValue(model, int.Parse(page));

            if (!string.IsNullOrWhiteSpace(pageSize))
                model.GetType().GetProperty("PageSize")?.SetValue(model, int.Parse(pageSize));
        }

        private static void ExtractSelect(object model, ModelBindingContext bindingContext)
        {
            var select = bindingContext.ValueProvider.GetValue("select")?.AttemptedValue;

            if (!string.IsNullOrWhiteSpace(select)) model.GetType().GetProperty("Select")?.SetValue(model, @select);
        }


        private static void ExtractOrder(object model, ModelBindingContext bindingContext)
        {
            var order = bindingContext.ValueProvider.GetValue("order")?.AttemptedValue;

            if (!string.IsNullOrWhiteSpace(order)) model.GetType().GetProperty("Order")?.SetValue(model, order);
        }

        private static void ExtractFilters(object model, ModelBindingContext bindingContext)
        {
            var filter = bindingContext.ValueProvider.GetValue("filter")?.AttemptedValue;

            if (filter == null)
                filter = bindingContext.ValueProvider.GetValue("query")?.AttemptedValue;

            if (!string.IsNullOrWhiteSpace(filter)) model.GetType().GetProperty("Filter")?.SetValue(model, filter);
        }
    }
}