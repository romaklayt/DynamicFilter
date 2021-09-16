using System;
using System.Linq.Expressions;

namespace romaklayt.DynamicFilter.Parser.Models
{
    public class ExpressionDynamicFilter<T> : ExpressionDynamicFilter<T, T>
    {
    }

    public class ExpressionDynamicFilter<TSource, TTarget>
    {
        public Expression<Func<TSource, bool>> Filter { get; set; }
        public Expression<Func<TSource, object>> Order { get; set; }
        public Expression<Func<TSource, TTarget>> Select { get; set; }
        public OrderType OrderType { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}