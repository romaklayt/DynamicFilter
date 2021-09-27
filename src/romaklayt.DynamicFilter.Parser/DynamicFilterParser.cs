using System;
using System.Linq.Expressions;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Parser
{
    public static class DynamicFilterParser
    {
        public static ExpressionDynamicFilter<TSource, TTarget> BindFilterExpressions<TSource, TTarget>(
            this BaseDynamicFilterModel filterModel)
        {
            if (filterModel == null) throw new ArgumentNullException(nameof(filterModel));

            var model = Activator.CreateInstance(typeof(ExpressionDynamicFilter<TSource, TTarget>));

            var itemType = typeof(ExpressionDynamicFilter<TSource, TTarget>).GenericTypeArguments[0];

            var parameter = Expression.Parameter(itemType, "x");

            DynamicComplexParser.ExtractFilters(model, filterModel, parameter, itemType);

            return model as ExpressionDynamicFilter<TSource, TTarget>;
        }
    }
}