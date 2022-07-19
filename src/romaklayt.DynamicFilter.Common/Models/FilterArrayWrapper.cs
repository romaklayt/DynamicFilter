using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace romaklayt.DynamicFilter.Common.Models;

public class FilterArrayWrapper
{
    private FilterArray FilterArray { get; }
    private List<FilterArrayWrapper> FilterArrayWrappers { get; }
    private List<FilterArrayLogicOperatorEnum> Operators { get; }

    public Expression Expression { get; }

    public FilterArrayWrapper(string wrap, Type type, ParameterExpression parameter)
    {
        var countMaxBraces = CountMaxBraces(wrap);
        var split = wrap.Split(Generate(countMaxBraces), StringSplitOptions.RemoveEmptyEntries);
        if (split.Length > 1 && countMaxBraces != 0)
        {
            FilterArrayWrappers = split.Select(s => new FilterArrayWrapper(s, type, countMaxBraces - 1, parameter))
                .ToList();
            Operators = Extensions.ParseOperators(wrap, split);
        }
        else
        {
            FilterArray = new FilterArray(wrap, type, parameter);
        }

        Expression = GetExpression();
    }

    public FilterArrayWrapper(string wrap, Type type, int depth, ParameterExpression parameter)
    {
        var split = wrap.Split(Generate(depth), StringSplitOptions.RemoveEmptyEntries);
        if (split.Length > 1 && depth != 0)
        {
            FilterArrayWrappers = split.Select(s => new FilterArrayWrapper(s, type, depth - 1, parameter)).ToList();
            Operators = Extensions.ParseOperators(wrap, split);
        }
        else
        {
            FilterArray = new FilterArray(wrap, type, parameter);
        }

        Expression = GetExpression();
    }

    private Expression GetExpression()
    {
        if (FilterArray != null) return FilterArray.Expression;

        var currentExpression = FilterArrayWrappers[0].Expression;
        if (FilterArrayWrappers.Count > 1)
            for (var i = 0; i < Operators.Count; i++)
                currentExpression = Operators[i] switch
                {
                    FilterArrayLogicOperatorEnum.And => Expression.And(currentExpression,
                        FilterArrayWrappers[i + 1].Expression),
                    FilterArrayLogicOperatorEnum.Or => Expression.Or(currentExpression,
                        FilterArrayWrappers[i + 1].Expression),
                    _ => currentExpression
                };

        return currentExpression;
    }

    private static string[] Generate(int braces)
    {
        var left = string.Empty;
        var right = string.Empty;
        for (var i = 0; i < braces; i++)
        {
            left += ")";
            right += "(";
        }

        var result = new List<string>();
        result.AddRange(FilterArrayLogicOperators.GetOperators().Select(s => $"{left}{s}{right}"));
        result.AddRange(FilterArrayLogicOperators.GetOperators().Select(s => $"{s}{right}"));
        result.AddRange(FilterArrayLogicOperators.GetOperators().Select(s => $"{left}{s}"));
        return result.ToArray();
    }

    private static int CountMaxBraces(string wrap)
    {
        var countMaxBraces = 0;
        var current = 0;
        foreach (var c in wrap)
            if (c == '(')
            {
                current++;
            }
            else
            {
                if (current > countMaxBraces) countMaxBraces = current;
                current = 0;
            }

        return countMaxBraces;
    }
}