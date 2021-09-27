using System;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Parser
{
    public static class DynamicSelectParser
    {
        public static ExpressionDynamicFilter<TSource, TTarget> BindFilterExpressions<TSource, TTarget>(
            this BaseDynamicSelectModel selectModel)
        {
            if (selectModel == null) throw new ArgumentNullException(nameof(selectModel));

            var model = Activator.CreateInstance(typeof(ExpressionDynamicFilter<TSource, TTarget>));

            DynamicComplexParser.ExtractSelect<TSource, TTarget>(model, selectModel);

            return model as ExpressionDynamicFilter<TSource, TTarget>;
        }
    }
}