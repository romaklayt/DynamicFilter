﻿using System.Web.Http;
using System.Web.Http.ValueProviders;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Factories;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Add custom value provider factory to services
            config.Services.Add(typeof(ValueProviderFactory), new FormValueProviderFactory());
            config.Services.Add(typeof(ValueProviderFactory), new MultipartValueProviderFactory());
            config.Services.Add(typeof(ValueProviderFactory), new HttpRequestMessageValueProviderFactory());

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{action}/{id}",
                new { id = RouteParameter.Optional }
            );
        }
    }
}