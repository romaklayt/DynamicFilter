using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi
{
    public class FormValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            return new FormValueProvider(actionContext);
        }
    }
}