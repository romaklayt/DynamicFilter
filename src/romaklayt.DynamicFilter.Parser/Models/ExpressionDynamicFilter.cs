using System;
using System.Linq.Expressions;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Parser.Models
{
    public class ExpressionDynamicFilter<T> : BaseDynamicFilter
    {
        public Expression<Func<T, bool>> Filter { get; set; }
        public Expression<Func<T, object>> Order { get; set; }
        public Expression<Func<T, T>> Select { get; set; }
        public OrderType OrderType { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}