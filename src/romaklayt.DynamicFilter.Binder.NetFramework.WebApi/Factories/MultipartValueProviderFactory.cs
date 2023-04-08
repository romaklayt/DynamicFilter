using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Providers;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Factories;

public class MultipartValueProviderFactory : ValueProviderFactory
{
    public override IValueProvider GetValueProvider(HttpActionContext actionContext) =>
        new MultipartValueProvider(actionContext);
}