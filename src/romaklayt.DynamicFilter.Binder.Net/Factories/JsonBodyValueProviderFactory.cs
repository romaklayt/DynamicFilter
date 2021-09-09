using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using romaklayt.DynamicFilter.Binder.Net.Providers;

namespace romaklayt.DynamicFilter.Binder.Net.Factories
{
    public class JsonBodyValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            context.ValueProviders.Add(new JsonBodyValueProvider(context));

            return Task.CompletedTask;
        }
    }
}