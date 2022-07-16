using System;
using System.Collections.Generic;
using System.Linq;
using romaklayt.DynamicFilter.Common.Models;

namespace romaklayt.DynamicFilter.Common;

public static class Extensions
{
    public static string GetLogicOperators(this string filter)
    {
        var split = filter.Split(FilterArrayLogicOperators.GetOperators(), StringSplitOptions.None);
        return RemoveSubstring(RemoveSubstring(filter, split.First()), split.Last());
    }

    public static string GetOperator(this string filter, string firstElement, string secondElement)
    {
        return RemoveSubstring(RemoveSubstring(filter, firstElement), secondElement);
    }

    public static string RemoveSubstring(string sourceString, string removeString)
    {
        var index = sourceString.IndexOf(removeString, StringComparison.InvariantCulture);
        return index < 0
            ? sourceString
            : sourceString.Remove(index, removeString.Length);
    }

    public static List<FilterArrayLogicOperatorEnum> ParseOperators(string filterArray, string[] split)
    {
        var op = split.Aggregate(filterArray, RemoveSubstring)
            .Where(c => c != '(' && c != ')').Select(c => c.ToString()).ToList();
        var t = string.Empty;
        var Operators = new List<FilterArrayLogicOperatorEnum>();
        foreach (var s in op)
        {
            if (t.Length < 2) t += s;

            if (t.Length == 2)
            {
                var ope = typeof(FilterArrayLogicOperators).GetFields().First(info =>
                    info.GetValue(null).ToString() == t).Name;
                Operators.Add((FilterArrayLogicOperatorEnum)Enum.Parse(typeof(FilterArrayLogicOperatorEnum), ope));
                t = string.Empty;
            }
        }

        return Operators;
    }
}