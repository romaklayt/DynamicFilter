using System;
using System.Linq.Expressions;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Parser
{
    public static class DynamicCounterModelParser
    {
        public static ExpressionDynamicFilter<TSource, TTarget> BindFilterExpressions<TSource, TTarget>(
            this BaseCountDynamicFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var model = Activator.CreateInstance(typeof(ExpressionDynamicFilter<TSource, TTarget>));

            var itemType = typeof(ExpressionDynamicFilter<TSource, TTarget>).GenericTypeArguments[0];

            var parameter = Expression.Parameter(itemType, "x");

            DynamicModelParser.ExtractFilters(model, filter, parameter, itemType);

            return model as ExpressionDynamicFilter<TSource, TTarget>;
        }
    }
}