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
        var page = (complexModel as IDynamicPaging).BindExpressions<T, T>();
        if (page.PageSize == default) page.PageSize = 10;
        if (page.Page == default) page.Page = 1;
        var count = applySorting ? await source.CountAsync(complexModel) : await source.CountAsync();
        var items = await source.Apply(complexModel, applyFiltering, applySorting, true, applySelect)
            .ToListAsync();
        return new PageModel<T>(items, count, page.Page, page.PageSize);
    }
}