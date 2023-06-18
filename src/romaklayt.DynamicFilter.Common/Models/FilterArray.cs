using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace romaklayt.DynamicFilter.Common.Models;

internal class FilterArray
{
    public FilterArray(string filterArray, Type type, Expression parameter)
    {
        var split = filterArray.Split(FilterArrayLogicOperators.GetOperators(),
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        FilterElements = split.Select(s => new FilterElement(s.Trim('(', ')'), type, parameter)).ToList();
        Operators = Extensions.ParseOperators(filterArray, split);
        Expression = GetExpression();
    }

    private List<FilterElement> FilterElements { get; }
    private List<FilterArrayLogicOperatorEnum> Operators { get; }
    public Expression Expression { get; }

    private Expression GetExpression()
    {
        var currentExpression = FilterElements[0].Expression;
        if (FilterElements.Count > 1)
            for (var i = 0; i < Operators.Count; i++)
                currentExpression = Operators[i] switch
                {
                    FilterArrayLogicOperatorEnum.And => Expression.And(currentExpression,
                        FilterElements[i + 1].Expression),
                    FilterArrayLogicOperatorEnum.Or => Expression.Or(currentExpression,
                        FilterElements[i + 1].Expression),
                    _ => currentExpression
                };

        return currentExpression;
    }
}