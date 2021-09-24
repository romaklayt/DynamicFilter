using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Parser
{
    public static class SelectedRender
    {
        /// <summary>
        ///     Return json string with only selected properties
        /// </summary>
        /// <param name="source">IEnumerable source</param>
        /// <param name="filter">Dynamic filter</param>
        /// <param name="formatting">Json string format</param>
        /// <typeparam name="T">IEnumerable type</typeparam>
        /// <returns>Json string</returns>
        public static string RenderOnlySelectedProperties<T>(this T source, BaseDynamicFilter filter,
            Formatting formatting = Formatting.Indented)
            where T : IEnumerable<object>
        {
            if (filter.Select != null)
            {
                var jArray = new List<object>();
                CheckRootMember<T>(filter);
                UpdateMembersPath(filter.Select, source.FirstOrDefault()?.GetType(), out var items, true);
                foreach (var o in source) jArray.Add(MapToDictionary(o, items));
                return JsonConvert.SerializeObject(jArray, formatting);
            }

            return JsonConvert.SerializeObject(source, formatting);
        }

        private static void CheckRootMember<T>(BaseDynamicFilter filter) where T : IEnumerable<object>
        {
            var selectedMembers =
                filter.Select.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (selectedMembers.Contains("root"))
            {
                selectedMembers.Remove("root");
                selectedMembers.AddRange(typeof(T).GenericTypeArguments[0].GetProperties()
                    .Where(info => IsSimple(info.PropertyType)).Select(info => FirstCharToLowerCase(info.Name)));
                filter.Select = string.Join(",", selectedMembers);
            }
        }

        private static void UpdateMembersPath(string includePropertiesString, Type type, out List<string> props,
            bool isroot = false)
        {
            props = new List<string>();
            foreach (var includeProperty in includePropertiesString.Split(new[] { ',' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                var prop = includeProperty.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (prop.Length > 1)
                {
                    var y = GetPropertyType(type, prop[0]);
                    if (y != null)
                    {
                        UpdateMembersPath(includeProperty.Split(new[] { '.' }, 2)[1], y, out var list);
                        if (list == null) continue;
                        if (isroot)
                        {
                            if (list.Any())
                                foreach (var prop1 in list)
                                    props.Add($"{includeProperty}.{prop1}");
                            else props.Add(includeProperty);
                        }
                        else
                        {
                            props.AddRange(list);
                        }
                    }
                    else
                    {
                        props.Add(includeProperty);
                    }
                }
                else
                {
                    var subtype = GetPropertyType(type, includeProperty);
                    if (IsSimple(subtype))
                    {
                        props.Add(includeProperty);
                        continue;
                    }

                    if (subtype.IsGenericList())
                    {
                        props.AddRange(subtype.GenericTypeArguments.First().GetProperties()
                            .Where(info => IsSimple(info.PropertyType))
                            .Select(info => $"{prop[0]}.{FirstCharToLowerCase(info.Name)}")
                            .ToList());
                        continue;
                    }

                    props.AddRange(subtype.GetProperties().Where(info => IsSimple(info.PropertyType))
                        .Select(info =>
                            isroot
                                ? $"{includeProperty}.{FirstCharToLowerCase(info.Name)}"
                                : FirstCharToLowerCase(info.Name)).ToList());
                }
            }
        }

        private static string FirstCharToLowerCase(string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
                return str;

            return char.ToLower(str[0]) + str.Substring(1);
        }

        private static Type GetPropertyType(Type type, string prop)
        {
            return type.GenericTypeArguments.FirstOrDefault()?.GetProperties()
                .FirstOrDefault(info => info.Name.ToLower() == prop.ToLower())?.PropertyType ?? type
                .GetProperties().FirstOrDefault(info => info.Name.ToLower() == prop.ToLower())?.PropertyType;
        }

        private static bool IsGenericList(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public static bool IsSimple(Type type)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }

        private static JObject MapToDictionary(object source, List<string> members)
        {
            var dictionary = new JObject();
            var parsedMembersList = members.Select(s => s.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
                .ToList();
            foreach (var parsedMembers in parsedMembersList) MapToDictionaryInternal(dictionary, source, parsedMembers);
            return dictionary;
        }

        private static void MapToDictionaryInternal(JObject jObject, object source, string[] members, int deep = 0)
        {
            if (source == null) return;
            var propertyInfo = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(info => info.Name.ToLower() == members.ElementAtOrDefault(deep)?.ToLower());
            if (propertyInfo == null) return;
            if (IsSimple(propertyInfo.PropertyType))
            {
                jObject.Add(members.ElementAt(deep),
                    JsonConvert.SerializeObject(propertyInfo.GetValue(source, null)).Trim('\\', '\"'));
            }
            else if (propertyInfo.GetValue(source, null) is IEnumerable)
            {
                var newArrayValues = new JArray();
                foreach (var o in (IEnumerable)propertyInfo.GetValue(source, null))
                {
                    var l = new JObject();
                    MapToDictionaryInternal(l, o, members, deep + 1);
                    newArrayValues.Add(l);
                }

                if (jObject.ContainsKey(members.ElementAt(deep)))
                {
                    var tempArray = jObject.GetValue(members.ElementAt(deep)) as JArray;
                    if (tempArray != null)
                        for (var index = 0; index < tempArray.Count; index++)
                            (tempArray[index] as JObject)?.Merge(newArrayValues[index]);

                    jObject.Remove(members.ElementAt(deep));
                    jObject.Add(members.ElementAt(deep), tempArray);
                }
                else
                {
                    jObject.Add(members.ElementAt(deep), newArrayValues);
                }
            }
            else
            {
                var obj = new JObject();
                MapToDictionaryInternal(obj, propertyInfo.GetValue(source, null), members, deep + 1);
                if (jObject.ContainsKey(members.ElementAt(deep)))
                {
                    var e = jObject.GetValue(members.ElementAt(deep));
                    (e as JObject)?.Merge(obj);
                    jObject.Remove(members.ElementAt(deep));
                    jObject.Add(members.ElementAt(deep), e);
                }
                else
                {
                    jObject.Add(members.ElementAt(deep), obj);
                }
            }
        }
    }
}