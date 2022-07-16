using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace romaklayt.DynamicFilter.Common.Models;

public class FilterArray
{
    public FilterArray(string filterArray, Type type, ParameterExpression parameter)
    {
        var split = filterArray.Split(FilterArrayLogicOperators.GetOperators(), StringSplitOptions.RemoveEmptyEntries);
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
                switch (Operators[i])
                {
                    case FilterArrayLogicOperatorEnum.And:
                        currentExpression = Expression.And(currentExpression, FilterElements[i + 1].Expression);
                        break;
                    case FilterArrayLogicOperatorEnum.Or:
                        currentExpression = Expression.Or(currentExpression, FilterElements[i + 1].Expression);
                        break;
                }

        return currentExpression;
    }
}