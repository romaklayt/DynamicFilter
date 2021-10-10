using System.Web.Mvc;
using System.Web.Routing;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new {controller = "User", action = "GetList", id = UrlParameter.Optional}
            );
        }
    }
}