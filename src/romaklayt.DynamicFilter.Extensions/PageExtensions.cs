using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Extensions;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class PageExtensions
{
    public static async Task<PageModel<TTarget>> ToPagedList<TSource, TTarget>(this IQueryable<TSource> source,
        ExpressionDynamicFilter<TSource, TTarget> filter, bool applyFiltering = true, bool applySorting = true,
        bool applySelect = true) where TSource : class where TTarget : class
    {
        if (filter.PageSize == default) filter.PageSize = 10;
        if (filter.Page == default) filter.Page = 1;
        var filteredEntities = await source.UseFilter(filter, applyPagination: false, applyFiltering: applyFiltering,
            applySelect: applySelect,
            applySorting: applySorting);
        var count = filteredEntities.Count();
        var items = filteredEntities.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();
        return await Task.FromResult(new PageModel<TTarget>(items, count, filter.Page, filter.PageSize));
    }


    public static async Task<PageModel<T>> ToPagedList<T>(this IEnumerable<T> source,
        BaseDynamicComplexModel complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applySelect = true)
        where T : class
    {
        return await source.AsQueryable().ToPagedList(complexModel.BindFilterExpressions<T, T>(), applyFiltering,
            applySorting, applySelect);
    }
}