using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Common.Interfaces;
using romaklayt.DynamicFilter.Parser;

namespace romaklayt.DynamicFilter.Extensions.Async;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class PageExtensions
{
    public static async Task<PageModel<T>> ToPageModel<T>(this IAsyncQueryable<T> source,
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

    public static async Task<PageModel<T>> ToPageModel<T>(this IAsyncEnumerable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applySelect = true) where T : class =>
        await source.AsAsyncQueryable().ToPageModel(complexModel, applyFiltering, applySorting, applySelect);

    public static async Task<PageFlatModel<T>> ToPageFlatModel<T>(this IAsyncEnumerable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applySelect = true) where T : class
    {
        var page = await source.AsAsyncQueryable().ToPageModel(complexModel, applyFiltering, applySorting, applySelect);
        return page.ToFlatModel();
    }
}