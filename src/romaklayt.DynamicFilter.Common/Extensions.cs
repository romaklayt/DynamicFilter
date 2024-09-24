using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using romaklayt.DynamicFilter.Common.Models;

namespace romaklayt.DynamicFilter.Common;

public static class Extensions
{
    private const string DefaultValue = @"\default";

    internal static string GetOperator(this string filter, string firstElement, string secondElement) =>
        RemoveSubstring(RemoveSubstring(filter, firstElement), secondElement, true);

    private static string RemoveSubstring(string sourceString, string removeString, bool reverse = false)
    {
        var index = reverse ? sourceString.LastIndexOf(removeString, StringComparison.Ordinal) : sourceString.IndexOf(removeString, StringComparison.Ordinal);
        return index < 0 ? sourceString : sourceString.Remove(index, removeString.Length);
    }

    internal static List<FilterArrayLogicOperatorEnum> ParseOperators(string filterArray, IEnumerable<string> split)
    {
        var op = split.Aggregate(filterArray, (s, s1) => RemoveSubstring(s, s1)).Where(c => c != '(' && c != ')' && c != ' ').Select(c => c.ToString()).ToList();
        var operatorTemp = string.Empty;
        var operators = new List<FilterArrayLogicOperatorEnum>();
        foreach (var s in op)
        {
            if (operatorTemp.Length < 2) operatorTemp += s;

            if (operatorTemp.Length != 2) continue;
            var ope = typeof(FilterArrayLogicOperators).GetFields().First(info => info.GetValue(null)?.ToString() == operatorTemp).Name;
            operators.Add((FilterArrayLogicOperatorEnum)Enum.Parse(typeof(FilterArrayLogicOperatorEnum), ope));
            operatorTemp = string.Empty;
        }

        return operators;
    }

    public static PageFlatModel<T> ToFlatModel<T>(this PageModel<T> pageModel) => new(pageModel.Items, pageModel.TotalCount, pageModel.CurrentPage, pageModel.PageSize);

    public static PageModel<T> ToNormalModel<T>(this PageFlatModel<T> pageModel) => new(pageModel, pageModel.TotalCount, pageModel.CurrentPage, pageModel.PageSize);

    public static object ParseValue(this Type type, string value)
    {
        if (value is DefaultValue or null) return GetDefaultValue(type);
        if (type.IsEnum)
            return Enum.Parse(type, value);
        return type == typeof(Guid) ? Guid.Parse(value) : TypeDescriptor.GetConverter(type).ConvertFrom(value);
    }

    private static object GetDefaultValue(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}