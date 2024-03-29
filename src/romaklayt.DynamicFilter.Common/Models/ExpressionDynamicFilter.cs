using System;
using System.Linq.Expressions;

namespace romaklayt.DynamicFilter.Common.Models;

public class ExpressionDynamicFilter<TSource, TTarget>
{
    public Expression<Func<TSource, bool>> Filter { get; set; }
    public string Order { get; set; }
    public Expression<Func<TSource, TTarget>> Select { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}