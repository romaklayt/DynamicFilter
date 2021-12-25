using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Providers;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Factories;

public class JsonBodyValueProviderFactory : ValueProviderFactory
{
    public override IValueProvider GetValueProvider(HttpActionContext actionContext)
    {
        return new JsonBodyValueProvider(actionContext);
    }
}