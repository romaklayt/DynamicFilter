using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Providers;

public class MultipartValueProvider : IValueProvider
{
    private readonly Collection<HttpContent> _values;

    public MultipartValueProvider(HttpActionContext actionContext)
    {
        if (actionContext == null) throw new ArgumentNullException(nameof(actionContext));

        _values = new Collection<HttpContent>();
        if (actionContext.Request.Content.IsMimeMultipartContent())
            _values = actionContext.Request.Content.ReadAsMultipartAsync().Result.Contents;
    }

    public bool ContainsPrefix(string prefix)
    {
        return _values.Any(content => content.Headers.Contains(prefix));
    }

    public ValueProviderResult GetValue(string key)
    {
        foreach (var contentPart in _values)
        {
            var contentDisposition = contentPart.Headers.ContentDisposition;
            if (contentDisposition.Name.Trim('"').Equals(key))
                return new ValueProviderResult(contentPart.ReadAsStringAsync().Result,
                    contentPart.ReadAsStringAsync().Result, CultureInfo.InvariantCulture);
        }

        return null;
    }
}