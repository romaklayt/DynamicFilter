using System.Web.Http;
using System.Web.Http.ValueProviders;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Factories;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi
{
    public static class DynamicFilterProviders
    {
        public static void AddDynamicFilterProviders(this HttpConfiguration config)
        {
            config.Services.Add(typeof(ValueProviderFactory), new FormValueProviderFactory());
            config.Services.Add(typeof(ValueProviderFactory), new MultipartValueProviderFactory());
            config.Services.Add(typeof(ValueProviderFactory), new JsonBodyValueProviderFactory());
        }
    }
}