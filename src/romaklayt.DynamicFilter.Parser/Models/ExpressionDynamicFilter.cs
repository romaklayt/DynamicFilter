using System;
using System.Linq.Expressions;

namespace romaklayt.DynamicFilter.Parser.Models
{
    public class ExpressionDynamicFilter<T>
    {
        public Expression<Func<T, bool>> Filter { get; set; }
        public Expression<Func<T, object>> Order { get; set; }
        public Expression<Func<T, T>> Select { get; set; }
        public OrderType OrderType { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool AsNoTracking { get; set; } = true;
    }
}