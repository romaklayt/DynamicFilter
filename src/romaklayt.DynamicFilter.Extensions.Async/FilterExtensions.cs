using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using romaklayt.DynamicFilter.Common.Interfaces;
using romaklayt.DynamicFilter.Parser;

namespace romaklayt.DynamicFilter.Extensions.Async;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class FilterExtensions
{
    public static IAsyncEnumerable<T> Apply<T>(this IAsyncEnumerable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applyPagination = true, bool applySelect = true) where T : class =>
        source.AsAsyncQueryable()
            .Apply(complexModel, applyFiltering, applySorting, applyPagination, applySelect);

    public static IAsyncQueryable<T> Apply<T>(this IAsyncQueryable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applyPagination = true, bool applySelect = true) where T : class
    {
        if (complexModel == null) return source;
        if (applyFiltering) source = source.ApplyFilter(complexModel);
        if (applySorting) source = source.ApplySorting(complexModel);
        if (applyPagination) source = source.ApplyPaging(complexModel);
        if (applySelect) source = source.ApplySelect(complexModel);

        return source;
    }

    public static IAsyncEnumerable<T> ApplyFilter<T>(this IAsyncEnumerable<T> source,
        IDynamicFilter complexModel) where T : class =>
        source.AsAsyncQueryable().ApplyFilter(complexModel);

    public static IAsyncQueryable<T> ApplyFilter<T>(this IAsyncQueryable<T> source,
        IDynamicFilter complexModel) where T : class
    {
        if (complexModel == null) return source;
        var filter = complexModel.BindExpressions<T, T>();
        return filter.Filter != null
            ? source.AsAsyncQueryable().Where(filter.Filter)
            : source.AsAsyncQueryable();
    }

    public static IAsyncEnumerable<T> ApplySorting<T>(this IAsyncEnumerable<T> source,
        IDynamicFilter complexModel) where T : class =>
        source.AsAsyncQueryable().ApplySorting(complexModel);

    public static IAsyncQueryable<T> ApplySorting<T>(this IAsyncQueryable<T> source,
        IDynamicFilter complexModel) where T : class
    {
        if (complexModel == null) return source;
        var filter = complexModel.BindExpressions<T, T>();
        if (filter.Order != null)
            source = source.DynamicOrderBy(filter.Order.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => new Tuple<string, bool>(s.TrimStart('-'), s.StartsWith("-"))).ToArray());
        return source;
    }

    public static IAsyncEnumerable<T> ApplySelect<T>(this IAsyncEnumerable<T> source,
        IDynamicSelect complexModel) where T : class =>
        source.AsAsyncQueryable().ApplySelect(complexModel);

    public static IAsyncQueryable<T> ApplySelect<T>(this IAsyncQueryable<T> source,
        IDynamicSelect complexModel) where T : class
    {
        if (complexModel == null) return source;
        var filter = complexModel.BindExpressions<T, T>();
        return filter.Select != null ? source.AsAsyncQueryable().Select(filter.Select) : source;
    }

    public static IAsyncEnumerable<T> ApplyPaging<T>(this IAsyncEnumerable<T> source,
        IDynamicPaging complexModel) where T : class =>
        source.AsAsyncQueryable().ApplyPaging(complexModel);

    public static IAsyncQueryable<T> ApplyPaging<T>(this IAsyncQueryable<T> source,
        IDynamicPaging complexModel) where T : class
    {
        if (complexModel == null) return source;
        var filter = complexModel.BindExpressions<T, T>();
        if (filter.PageSize == default && filter.Page == default)
            return source;
        if (filter.PageSize == default) filter.PageSize = 10;
        if (filter.Page == default) filter.Page = 1;
        return source.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);
    }
}