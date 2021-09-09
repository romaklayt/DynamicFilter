using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using romaklayt.DynamicFilter.Binder.NetFramework;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ValueProviderFactories.Factories.Add(new FormValueProviderFactory());
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
            ValueProviderFactories.Factories.Add(new RouteDataValueProviderFactory());
            ValueProviderFactories.Factories.Add(new QueryStringValueProviderFactory());
            ValueProviderFactories.Factories.Add(new JQueryFormValueProviderFactory());
            
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
