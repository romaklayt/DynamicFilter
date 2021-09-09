using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi
{
    public class MultipartValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            return new MultipartValueProvider(actionContext);
        }
    }
}