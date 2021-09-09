using System.Web.Http;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Add custom dynamic filter providers for web api
            config.AddDynamicFilterProviders();

            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{action}/{id}",
                new { id = RouteParameter.Optional }
            );
        }
    }
}