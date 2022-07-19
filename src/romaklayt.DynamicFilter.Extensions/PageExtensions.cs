using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Common.Interfaces;
using romaklayt.DynamicFilter.Parser;

namespace romaklayt.DynamicFilter.Extensions;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class PageExtensions
{
    public static async Task<PageModel<T>> ToPageModel<T>(this IEnumerable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applySelect = true) where T : class
    {
        return await source.AsQueryable().ToPageModel(complexModel, applyFiltering, applySorting, applySelect);
    }

    public static async Task<PageModel<T>> ToPageModel<T>(this IQueryable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applySelect = true) where T : class
    {
        var filter = (complexModel as IDynamicPaging).BindExpressions<T, T>();
        if (filter.PageSize == default) filter.PageSize = 10;
        if (filter.Page == default) filter.Page = 1;
        var filteredEntities =
            source.Apply(complexModel, applyFiltering, applySorting, false, applySelect).AsQueryable();
        var count = filteredEntities.Count();
        var items = filteredEntities.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();
        return await Task.FromResult(new PageModel<T>(items, count, filter.Page, filter.PageSize));
    }
}