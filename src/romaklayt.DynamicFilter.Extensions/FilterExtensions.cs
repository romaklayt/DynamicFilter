using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Extensions;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class FilterExtensions
{
    public static async Task<IQueryable<TTarget>> UseFilter<TSource, TTarget>(this IQueryable<TSource> source,
        ExpressionDynamicFilter<TSource, TTarget> filter, bool applyFiltering = true, bool applySorting = true,
        bool applyPagination = true, bool applySelect = true) where TSource : class where TTarget : class
    {
        var queryable = filter.Filter != null && applyFiltering
            ? source.Where(filter.Filter).AsQueryable()
            : source.AsQueryable();
        if (filter.Order != null && applySorting)
            queryable = queryable.DynamicOrderBy(filter.Order.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                {
                    var split = s.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                    return new Tuple<string, bool>(split.First(),
                        split.Length > 1 && split[1].ToLower().Contains("desc"));
                }).ToArray());
        var result = filter.Select != null && applySelect ? queryable.Select(filter.Select) : queryable.Cast<TTarget>();
        if (filter.PageSize == default && filter.Page == default || !applyPagination)
            return await Task.FromResult(result);
        if (filter.PageSize == default) filter.PageSize = 10;
        if (filter.Page == default) filter.Page = 1;
        return await Task.FromResult(result.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize));
    }

    public static async Task<IEnumerable<T>> UseFilter<T>(this IEnumerable<T> source,
        BaseDynamicComplexModel complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applyPagination = true, bool applySelect = true) where T : class
    {
        return await source.AsQueryable().UseFilter(complexModel.BindFilterExpressions<T, T>(), applyFiltering,
            applySorting, applyPagination, applySelect);
    }

    public static async Task<IEnumerable<T>> UseFilter<T>(this IEnumerable<T> source,
        BaseDynamicFilterModel filterModel, bool applyFiltering = true, bool applySorting = true,
        bool applySelect = true) where T : class
    {
        return await source.AsQueryable().UseFilter(filterModel.BindFilterExpressions<T, T>(), applyFiltering,
            applySorting, false, applySelect);
    }

    public static async Task<IEnumerable<T>> UseFilter<T>(this IEnumerable<T> source,
        BaseDynamicSelectModel selectModel) where T : class
    {
        return await source.AsQueryable().UseFilter(selectModel.BindFilterExpressions<T, T>(), false,
            false, false);
    }
}