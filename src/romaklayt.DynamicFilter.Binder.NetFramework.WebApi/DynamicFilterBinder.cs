using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Newtonsoft.Json;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi
{
    public class DynamicFilterBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var model = Activator.CreateInstance(bindingContext.ModelType);
            
            ExtractFilters(model, bindingContext);

            ExtractOrder(model, bindingContext);

            ExtractPagination(model, bindingContext);

            ExtractSelect(model, bindingContext);
            
            bindingContext.Model = model;
            
            return true;
        }
        private static T DeserializeObjectFromJson<T>(string json)
        {
            var binder = new TypeNameSerializationBinder("");

            var obj = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Binder = binder
            });
            return obj;
        }
        private static string ExtractRequestJson(HttpActionContext actionContext)
        {
            var content = actionContext.Request.Content;
            string json = content.ReadAsStringAsync().Result;
            return json;
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