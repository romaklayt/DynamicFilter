﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using Newtonsoft.Json;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Models;

namespace romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Providers;

public class JsonBodyValueProvider : IValueProvider
{
    private readonly Dictionary<string, string> _dictionary;

    public JsonBodyValueProvider(HttpActionContext actionContext)
    {
        if (actionContext == null) throw new ArgumentNullException(nameof(actionContext));
        var json = ExtractRequestJson(actionContext);
        _dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        if (!string.IsNullOrWhiteSpace(json))
        {
            var model = DeserializeObjectFromJson<DynamicComplexModel>(json);
            if (model != null)
                _dictionary = model.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .ToDictionary(prop => prop.Name.ToLower(), prop => prop.GetValue(model, null) as string);
        }
    }

    public bool ContainsPrefix(string prefix) => _dictionary.ContainsKey(prefix);

    public ValueProviderResult GetValue(string key)
    {
        if (_dictionary.TryGetValue(key.ToLower(), out var value))
            if (!string.IsNullOrEmpty(value))
                return new ValueProviderResult(value, value, CultureInfo.InvariantCulture);
        return null;
    }

    private static object DeserializeObjectFromJson<T>(string json)
    {
        try
        {
            var obj = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            return obj;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string ExtractRequestJson(HttpActionContext actionContext)
    {
        var content = actionContext.Request.Content;
        var json = content.ReadAsStringAsync().Result;
        return json;
    }
}