using System.Web.Mvc;

namespace romaklayt.DynamicFilter.Binder.NetFramework.Mvc
{
    public static class DynamicFilterProviders
    {
        public static void AddProviders()
        {
            ValueProviderFactories.Factories.Add(new FormValueProviderFactory());
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
            ValueProviderFactories.Factories.Add(new RouteDataValueProviderFactory());
            ValueProviderFactories.Factories.Add(new QueryStringValueProviderFactory());
            ValueProviderFactories.Factories.Add(new JQueryFormValueProviderFactory());
        }
    }
}