using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using Newtonsoft.Json;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi
{
    public class HttpRequestMessageValueProvider : IValueProvider
    {
        private readonly HttpRequestMessage _values;
        private readonly object _model;
        private readonly Dictionary<string, string> _dictionary;

        public HttpRequestMessageValueProvider(HttpActionContext actionContext)
        {
            if (actionContext == null) throw new ArgumentNullException(nameof(actionContext));
            var json = ExtractRequestJson(actionContext);
            _dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            _values = new ();
            if (!string.IsNullOrWhiteSpace(json))
            {
                _model = DeserializeObjectFromJson<DynamicFilterModel>(json);
                if (_model != null)
                {
                    _dictionary = _model.GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .ToDictionary(prop => prop.Name.ToLower(), prop => prop.GetValue(_model, null) as string);
                }
            }
                
        }

        public bool ContainsPrefix(string prefix)
        {
            return _dictionary.ContainsKey(prefix);
        }

        public ValueProviderResult GetValue(string key)
        {
            if (_dictionary.TryGetValue(key.ToLower(), out var value))
            {
                if (!string.IsNullOrEmpty(value))
                    return new ValueProviderResult(value, value, CultureInfo.InvariantCulture);
            }
            return null;
        }
        private static object DeserializeObjectFromJson<T>(string json)
        {
            var binder = new TypeNameSerializationBinder("");

            try
            {
                var obj = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Binder = binder
                });
                return obj;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        private static string ExtractRequestJson(HttpActionContext actionContext)
        {
            var content = actionContext.Request.Content;
            string json = content.ReadAsStringAsync().Result;
            return json;
        }
    }
}