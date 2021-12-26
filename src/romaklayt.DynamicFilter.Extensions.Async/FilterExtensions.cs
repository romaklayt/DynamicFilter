using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Extensions.Async;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class FilterExtensions
{
    public static async Task<IAsyncQueryable<TTarget>> UseFilter<TSource, TTarget>(this IAsyncQueryable<TSource> source,
        ExpressionDynamicFilter<TSource, TTarget> filter, bool applyFiltering = true, bool applySorting = true,
        bool applyPagination = true, bool applySelect = true) where TSource : class where TTarget : class
    {
        var queryable = filter.Filter != null && applyFiltering
            ? source.Where(filter.Filter).AsAsyncQueryable()
            : source.AsAsyncQueryable();
        if (filter.Order != null && applySorting)
            queryable = queryable.DynamicOrderBy(filter.Order.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                {
                    var split = s.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                    return new Tuple<string, bool>(split.First(),
                        split.Count() > 1 && split[1].ToLower().Contains("desc"));
                }).ToArray());
        var result = filter.Select != null && applySelect ? queryable.Select(filter.Select) : queryable.Cast<TTarget>();
        if (filter.PageSize == default && filter.Page == default || !applyPagination)
            return await Task.FromResult(result);
        if (filter.PageSize == default) filter.PageSize = 10;
        if (filter.Page == default) filter.Page = 1;
        return await Task.FromResult(result.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize));
    }

    public static async Task<IAsyncEnumerable<T>> UseFilter<T>(this IAsyncEnumerable<T> source,
        BaseDynamicComplexModel complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applyPagination = true, bool applySelect = true) where T : class
    {
        return await source.AsAsyncQueryable().UseFilter(complexModel.BindFilterExpressions<T, T>(), applyFiltering,
            applySorting, applyPagination, applySelect);
    }

    public static async Task<IAsyncEnumerable<T>> UseFilter<T>(this IAsyncEnumerable<T> source,
        BaseDynamicFilterModel filterModel, bool applyFiltering = true, bool applySorting = true,
        bool applySelect = true) where T : class
    {
        return await source.AsAsyncQueryable().UseFilter(filterModel.BindFilterExpressions<T, T>(), applyFiltering,
            applySorting, false, applySelect);
    }

    public static async Task<IAsyncEnumerable<T>> UseFilter<T>(this IAsyncEnumerable<T> source,
        BaseDynamicSelectModel selectModel) where T : class
    {
        return await source.AsAsyncQueryable().UseFilter(selectModel.BindFilterExpressions<T, T>(), false,
            false, false, true);
    }
}