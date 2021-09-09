using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace romaklayt.DynamicFilter.Binder.Net
{
    public class DynamicFilterBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var model = Activator.CreateInstance(bindingContext.ModelType);

            ExtractFilters(model, bindingContext);

            ExtractOrder(model, bindingContext);

            ExtractPagination(model, bindingContext);

            ExtractSelect(model, bindingContext);

            bindingContext.Result = ModelBindingResult.Success(model);

            return Task.CompletedTask;
        }

        private static void ExtractPagination(object model, ModelBindingContext bindingContext)
        {
            var page = bindingContext.ValueProvider.GetValue("page").FirstValue;
            var pageSize = bindingContext.ValueProvider.GetValue("pagesize").FirstValue;

            if (!string.IsNullOrWhiteSpace(page))
                model.GetType().GetProperty("Page")?.SetValue(model, int.Parse(page));

            if (!string.IsNullOrWhiteSpace(pageSize))
                model.GetType().GetProperty("PageSize")?.SetValue(model, int.Parse(pageSize));
        }

        private static void ExtractSelect(object model, ModelBindingContext bindingContext)
        {
            var select = bindingContext.ValueProvider.GetValue("select").FirstValue;

            if (!string.IsNullOrWhiteSpace(select)) model.GetType().GetProperty("Select")?.SetValue(model, select);
        }


        private static void ExtractOrder(object model, ModelBindingContext bindingContext)
        {
            var order = bindingContext.ValueProvider.GetValue("order").FirstValue;

            if (!string.IsNullOrWhiteSpace(order)) model.GetType().GetProperty("Order")?.SetValue(model, order);
        }

        private static void ExtractFilters(object model, ModelBindingContext bindingContext)
        {
            var filter = bindingContext.ValueProvider.GetValue("filter").FirstValue;

            if (filter == null)
                filter = bindingContext.ValueProvider.GetValue("query").FirstValue;

            if (!string.IsNullOrWhiteSpace(filter)) model.GetType().GetProperty("Filter")?.SetValue(model, filter);
        }
    }
}