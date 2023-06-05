using System;
using System.Collections.Generic;
using System.Linq;
using romaklayt.DynamicFilter.Common.Interfaces;
using romaklayt.DynamicFilter.Parser;

namespace romaklayt.DynamicFilter.Extensions.EntityFrameworkCore;

public static class FilterExtensions
{
    public static IEnumerable<T> Apply<T>(this IEnumerable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true,
        bool applyPagination = true, bool applySelect = true) where T : class =>
        source.AsQueryable().Apply(complexModel, applyFiltering, applyPagination, applySelect);

    public static IQueryable<T> Apply<T>(this IQueryable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true,
        bool applyPagination = true, bool applySelect = true) where T : class
    {
        if (complexModel == null) return source;
        if (applyFiltering) source = source.ApplyFilter(complexModel);
        if (applyPagination) source = source.ApplyPaging(complexModel);
        if (applySelect) source = source.ApplySelect(complexModel);
        return source;
    }

    public static IEnumerable<T> ApplyFilter<T>(this IEnumerable<T> source,
        IDynamicFilter complexModel) where T : class =>
        source.AsQueryable().ApplyFilter(complexModel);

    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> source,
        IDynamicFilter complexModel) where T : class
    {
        if (complexModel == null) return source;
        var filter = complexModel.BindExpressions<T, T>();
        if (filter.Filter != null)
            source = source.AsQueryable().Where(filter.Filter);
        if (filter.Order != null)
            source = source.DynamicOrderBy(filter.Order.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => new Tuple<string, bool>(s.TrimStart('-'), s.StartsWith("-"))).ToArray());
        return source;
    }

    public static IEnumerable<T> ApplySelect<T>(this IEnumerable<T> source,
        IDynamicSelect complexModel) where T : class =>
        source.AsQueryable().ApplySelect(complexModel);

    public static IQueryable<T> ApplySelect<T>(this IQueryable<T> source,
        IDynamicSelect complexModel) where T : class
    {
        if (complexModel == null) return source;
        var filter = complexModel.BindExpressions<T, T>();
        return filter.Select != null ? source.AsQueryable().Select(filter.Select) : source;
    }

    public static IEnumerable<T> ApplyPaging<T>(this IEnumerable<T> source,
        IDynamicPaging complexModel) where T : class =>
        source.AsQueryable().ApplyPaging(complexModel);

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> source,
        IDynamicPaging complexModel) where T : class
    {
        if (complexModel == null) return source;
        var filter = complexModel.BindExpressions<T, T>();
        if (filter.PageSize == -1 && filter.Page == default)
            return source;
        if (filter.PageSize == default) filter.PageSize = 10;
        if (filter.Page == default) filter.Page = 1;
        return source.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);
    }
}