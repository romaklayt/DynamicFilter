using System.Web.Mvc;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}