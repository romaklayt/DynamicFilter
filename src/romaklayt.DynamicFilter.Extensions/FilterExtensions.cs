using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using romaklayt.DynamicFilter.Common.Interfaces;
using romaklayt.DynamicFilter.Parser;

namespace romaklayt.DynamicFilter.Extensions;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class FilterExtensions
{
    public static IEnumerable<T> Apply<T>(this IEnumerable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applyPagination = true, bool applySelect = true) where T : class
    {
        return source.ApplyFilter(complexModel, applyFiltering, applySorting)
            .ApplySelect(complexModel, applySelect).ApplyPaging(complexModel, applyPagination);
    }

    public static IEnumerable<T> ApplyFilter<T>(this IEnumerable<T> source,
        IDynamicFilter complexModel, bool applyFiltering = true, bool applySorting = true) where T : class
    {
        var filter = complexModel.BindFilterExpressions<T, T>();
        var queryable = filter.Filter != null && applyFiltering
            ? source.AsQueryable().Where(filter.Filter)
            : source.AsQueryable();
        if (filter.Order != null && applySorting)
            queryable = queryable.DynamicOrderBy(filter.Order.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                {
                    var split = s.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                    return new Tuple<string, bool>(split.First(),
                        split.Length > 1 && split[1].ToLower().Contains("desc"));
                }).ToArray());
        return queryable;
    }

    public static IEnumerable<T> ApplySelect<T>(this IEnumerable<T> source,
        IDynamicSelect complexModel, bool applySelect = true) where T : class
    {
        var filter = complexModel.BindFilterExpressions<T, T>();
        var result = filter.Select != null && applySelect ? source.AsQueryable().Select(filter.Select) : source;
        return result;
    }

    public static IEnumerable<T> ApplyPaging<T>(this IEnumerable<T> source,
        IDynamicPaging complexModel,
        bool applyPagination = true) where T : class
    {
        var filter = complexModel.BindFilterExpressions<T, T>();
        if (filter.PageSize == default && filter.Page == default || !applyPagination)
            return source;
        if (filter.PageSize == default) filter.PageSize = 10;
        if (filter.Page == default) filter.Page = 1;
        return source.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);
    }
}