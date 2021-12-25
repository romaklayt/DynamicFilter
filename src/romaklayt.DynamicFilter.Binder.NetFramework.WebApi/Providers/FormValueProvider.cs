using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Providers;

public class FormValueProvider : IValueProvider
{
    private readonly NameValueCollection _values;

    public FormValueProvider(HttpActionContext actionContext)
    {
        if (actionContext == null) throw new ArgumentNullException(nameof(actionContext));

        _values = new NameValueCollection();
        if (actionContext.Request.Content.IsFormData())
            _values = actionContext.Request.Content.ReadAsFormDataAsync().Result;
    }

    public bool ContainsPrefix(string prefix)
    {
        return _values.AllKeys.Contains(prefix);
    }

    public ValueProviderResult GetValue(string key)
    {
        var value = _values.Get(key);
        if (!string.IsNullOrEmpty(value))
            return new ValueProviderResult(value, value, CultureInfo.InvariantCulture);
        return null;
    }
}