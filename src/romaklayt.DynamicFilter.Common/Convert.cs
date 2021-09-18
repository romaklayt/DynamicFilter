using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace romaklayt.DynamicFilter.Common
{
    public static class Convert
    {
        public static string GetOnlySelectedProperties<T>(this T source, BaseDynamicFilter filter)
            where T : IEnumerable<object>
        {
            if (filter.Select != null)
            {
                var jArray = new JArray();
                var selectedMembers =
                    filter.Select.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (selectedMembers.Contains("root"))
                {
                    selectedMembers.Remove("root");
                    selectedMembers.AddRange(typeof(T).GenericTypeArguments[0].GetProperties()
                        .Where(info => IsSimple(info.PropertyType))
                        .Select(info => info.Name));
                    filter.Select = string.Join(",", selectedMembers);
                }

                foreach (var o in source)
                {
                    var jObject = new JObject();
                    foreach (var s in filter.Select.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        jObject.Merge(MapToDictionaryInternal(new ObjectConvertInfo
                        {
                            ConvertObject = o,
                            IncludeProperties = s
                        }));
                    jArray.Add(jObject);
                }

                return JsonConvert.SerializeObject(jArray, Formatting.Indented);
            }

            return JsonConvert.SerializeObject(source, Formatting.Indented);
        }

        private static JObject MapToDictionaryInternal(ObjectConvertInfo objectConvertInfo)
        {
            var jObjectInternal = new JObject();
            var propertyName = objectConvertInfo.IncludeProperties
                .Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (propertyName != null)
            {
                var propertyInfo = objectConvertInfo.ConvertObject.GetType().GetProperties().FirstOrDefault(info =>
                    info.Name.ToLowerInvariant().Contains(propertyName.ToLowerInvariant()));
                if (propertyInfo != null)
                {
                    var key = propertyInfo.Name;
                    key = key.Replace(key.First(), char.ToLowerInvariant(key.First()));
                    var value = propertyInfo.GetValue(objectConvertInfo.ConvertObject, null);
                    if (value == null)
                    {
                        jObjectInternal.Add(key, null);
                        return jObjectInternal;
                    }

                    var valueType = value.GetType();
                    if (IsSimple(valueType))
                    {
                        jObjectInternal[key] = JsonConvert.SerializeObject(value).Trim('\\', '\"');
                    }
                    else if (value is IEnumerable enumerable)
                    {
                        var array = new JArray();
                        foreach (var data in enumerable)
                        {
                            var j = new JObject();
                            foreach (var s in CheckMembers(objectConvertInfo, data)
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                j.Merge(MapToDictionaryInternal(new ObjectConvertInfo
                                    { ConvertObject = data, IncludeProperties = s }));
                            array.Add(j);
                        }

                        jObjectInternal.Add(key, array);
                    }
                    else
                    {
                        var j = new JObject();
                        foreach (var s in CheckMembers(objectConvertInfo, value)
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            j.Merge(MapToDictionaryInternal(new ObjectConvertInfo
                                { ConvertObject = value, IncludeProperties = s }));
                        jObjectInternal.Add(key, j);
                    }
                }
                else
                {
                    jObjectInternal.Add(propertyName, null);
                }
            }

            return jObjectInternal;
        }

        private static string CheckMembers(ObjectConvertInfo objectConvertInfo, object value)
        {
            var temp = objectConvertInfo.IncludeProperties.DelFistElem();
            if (string.IsNullOrWhiteSpace(temp))
                temp = string.Join(",",
                    value.GetType().GetProperties().Where(info => IsSimple(info.PropertyType))
                        .Select(info => info.Name).ToList());
            return temp;
        }

        private static string DelFistElem(this string list)
        {
            var items = list.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return string.Join(".", items.Skip(1).ToList());
        }

        public static bool IsSimple(Type type)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }
    }
}