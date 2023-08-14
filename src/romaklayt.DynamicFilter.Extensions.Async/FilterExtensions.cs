using System;
using System.Collections.Generic;
using System.Linq;
using romaklayt.DynamicFilter.Common.Interfaces;
using romaklayt.DynamicFilter.Parser;

namespace romaklayt.DynamicFilter.Extensions.Async;

public static class FilterExtensions
{
    public static IAsyncEnumerable<T> Apply<T>(this IAsyncEnumerable<T> source, IDynamicComplex complexModel, bool applyFiltering = true, bool applyPagination = true,
        bool applySelect = true) where T : class =>
        source.AsAsyncQueryable().Apply(complexModel, applyFiltering, applyPagination, applySelect);

    public static IAsyncQueryable<T> Apply<T>(this IAsyncQueryable<T> source, IDynamicComplex complexModel, bool applyFiltering = true, bool applyPagination = true,
        bool applySelect = true) where T : class
    {
        if (complexModel == null) return source;
        if (applyFiltering) source = source.ApplyFilter(complexModel);
        if (applyPagination) source = source.ApplyPaging(complexModel);
        if (applySelect) source = source.ApplySelect(complexModel);
        return source;
    }

    public static IAsyncEnumerable<T> ApplyFilter<T>(this IAsyncEnumerable<T> source, IDynamicFilter complexModel, bool applySorting = true) where T : class =>
        source.AsAsyncQueryable().ApplyFilter(complexModel, applySorting);

    public static IAsyncQueryable<T> ApplyFilter<T>(this IAsyncQueryable<T> source, IDynamicFilter complexModel, bool applySorting = true) where T : class
    {
        if (complexModel == null) return source;
        var filter = complexModel.BindExpressions<T, T>();
        if (filter.Filter != null)
            source = source.Where(filter.Filter);
        if (filter.Order != null && applySorting)
            source = source.DynamicOrderBy(filter.Order.Split(new[] { ',' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(s => new Tuple<string, bool>(s.TrimStart('-'), s.StartsWith("-")))
                .ToArray());
        return source;
    }

    public static IAsyncEnumerable<T> ApplySelect<T>(this IAsyncEnumerable<T> source, IDynamicSelect complexModel) where T : class =>
        source.AsAsyncQueryable().ApplySelect(complexModel);

    public static IAsyncQueryable<T> ApplySelect<T>(this IAsyncQueryable<T> source, IDynamicSelect complexModel) where T : class
    {
        if (complexModel == null) return source;
        var filter = complexModel.BindExpressions<T, T>();
        return filter.Select != null ? source.AsAsyncQueryable().Select(filter.Select) : source;
    }

    public static IAsyncEnumerable<T> ApplyPaging<T>(this IAsyncEnumerable<T> source, IDynamicPaging complexModel) where T : class =>
        source.AsAsyncQueryable().ApplyPaging(complexModel);

    public static IAsyncQueryable<T> ApplyPaging<T>(this IAsyncQueryable<T> source, IDynamicPaging complexModel) where T : class
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