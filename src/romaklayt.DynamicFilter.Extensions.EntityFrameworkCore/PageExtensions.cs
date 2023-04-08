using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Common.Interfaces;
using romaklayt.DynamicFilter.Parser;

namespace romaklayt.DynamicFilter.Extensions.EntityFrameworkCore;

public static class PageExtensions
{
    public static async Task<PageModel<T>> ToPageModel<T>(this IEnumerable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applySelect = true) where T : class =>
        await source.AsQueryable().ToPageModel(complexModel, applyFiltering, applySorting, applySelect);

    public static async Task<PageModel<T>> ToPageModel<T>(this IQueryable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applySelect = true) where T : class
    {
        var filter = (complexModel as IDynamicPaging).BindExpressions<T, T>();
        if (filter.PageSize == default) filter.PageSize = 10;
        if (filter.Page == default) filter.Page = 1;
        var filteredEntities = applyFiltering ? source.ApplyFilter(complexModel).AsQueryable() : source.AsQueryable();
        var count = await filteredEntities.CountAsync();
        var items = await filteredEntities.Apply(complexModel, false, applySorting, true, applySelect)
            .ToListAsync();
        return new PageModel<T>(items, count, filter.Page, filter.PageSize);
    }
}