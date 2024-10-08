﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using romaklayt.DynamicFilter.Common.Models;

namespace romaklayt.DynamicFilter.Binder.Net.Providers;

public class JsonBodyValueProvider : IValueProvider
{
    private readonly Dictionary<string, string> _dictionary;
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public JsonBodyValueProvider(ValueProviderFactoryContext actionContext)
    {
        if (actionContext == null) throw new ArgumentNullException(nameof(actionContext));
        var json = ExtractRequestJson(actionContext).Result;
        _dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        if (!string.IsNullOrWhiteSpace(json))
        {
            var model = DeserializeObjectFromJson<DynamicComplexModel>(json);
            if (model != null)
                _dictionary = model.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .ToDictionary(prop => prop.Name.ToUpper(), prop => prop.GetValue(model, null) as string);
        }
    }

    public bool ContainsPrefix(string prefix) => _dictionary.ContainsKey(prefix);

    public ValueProviderResult GetValue(string key)
    {
        if (_dictionary.TryGetValue(key.ToUpper(), out var value))
            if (!string.IsNullOrEmpty(value))
                return new ValueProviderResult(value, CultureInfo.InvariantCulture);
        return new ValueProviderResult();
    }

    private static object DeserializeObjectFromJson<T>(string json)
    {
        try
        {
            var obj = JsonSerializer.Deserialize<T>(json, Options);
            return obj;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task<string> ExtractRequestJson(ValueProviderFactoryContext actionContext)
    {
        var content = actionContext.ActionContext.HttpContext.Request.Body;
        using var stream = new StreamReader(content);
        var body = await stream.ReadToEndAsync();
        return body;
    }
}