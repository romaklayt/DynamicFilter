using System;
using System.Collections.Generic;
using System.Linq;
using romaklayt.DynamicFilter.Common.Models;

namespace romaklayt.DynamicFilter.Common;

public static class Extensions
{
    public static string GetOperator(this string filter, string firstElement, string secondElement)
    {
        return RemoveSubstring(RemoveSubstring(filter, firstElement), secondElement);
    }

    private static string RemoveSubstring(string sourceString, string removeString)
    {
        var index = sourceString.IndexOf(removeString, StringComparison.InvariantCulture);
        return index < 0
            ? sourceString
            : sourceString.Remove(index, removeString.Length);
    }

    public static List<FilterArrayLogicOperatorEnum> ParseOperators(string filterArray, IEnumerable<string> split)
    {
        var op = split.Aggregate(filterArray, RemoveSubstring)
            .Where(c => c != '(' && c != ')').Select(c => c.ToString()).ToList();
        var operatorTemp = string.Empty;
        var operators = new List<FilterArrayLogicOperatorEnum>();
        foreach (var s in op)
        {
            if (operatorTemp.Length < 2) operatorTemp += s;

            if (operatorTemp.Length != 2) continue;
            var ope = typeof(FilterArrayLogicOperators).GetFields().First(info =>
                info.GetValue(null).ToString() == operatorTemp).Name;
            operators.Add((FilterArrayLogicOperatorEnum)Enum.Parse(typeof(FilterArrayLogicOperatorEnum), ope));
            operatorTemp = string.Empty;
        }

        return operators;
    }
}