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
    public static async Task<PageModel<T>> ToPageModel<T>(this IAsyncEnumerable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true,
        bool applySelect = true) where T : class =>
        await source.AsAsyncQueryable().ToPageModel(complexModel, applyFiltering, applySelect);

    public static async Task<PageFlatModel<T>> ToPageFlatModel<T>(this IAsyncEnumerable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true,
        bool applySelect = true) where T : class
    {
        var page = await source.AsAsyncQueryable().ToPageModel(complexModel, applyFiltering, applySelect);
        return page.ToFlatModel();
    }

    public static async Task<PageModel<T>> ToPageModel<T>(this IAsyncQueryable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true,
        bool applySelect = true) where T : class
    {
        var page = await GetPageInfo(source, complexModel);
        source = source.ApplyPaging(complexModel);
        if (applyFiltering) source = source.ApplyFilter(complexModel);
        if (applySelect) source = source.ApplySelect(complexModel);
        return new PageModel<T>(await source.ToListAsync(), page.count, page.page, page.pageSize);
    }

    public static async Task<PageModel<T>> ToPageModel<T>(this IAsyncQueryable<T> source,
        IDynamicPaging complexModel, bool applyFiltering = true) where T : class
    {
        var page = await GetPageInfo(source, complexModel);
        source = source.ApplyPaging(complexModel);
        if (applyFiltering) source = source.ApplyFilter(complexModel);
        return new PageModel<T>(await source.ToListAsync(), page.count, page.page, page.pageSize);
    }

    private static async Task<(int page, int pageSize, int count)> GetPageInfo<T>(IAsyncQueryable<T> source,
        IDynamicPaging complexModel) where T : class
    {
        var page = complexModel.BindExpressions<T, T>();
        if (page.PageSize == default) page.PageSize = 10;
        if (page.Page == default) page.Page = 1;
        var count = await source.CountAsync(complexModel);
        return (page.Page, page.PageSize, count);
    }

    public static async Task<PageModel<T>> ToPageModel<T>(this IAsyncEnumerable<T> source,
        IDynamicPaging complexModel, bool applyFiltering = true) where T : class =>
        await source.AsAsyncQueryable().ToPageModel(complexModel, applyFiltering);

    public static async Task<PageFlatModel<T>> ToPageFlatModel<T>(this IAsyncEnumerable<T> source,
        IDynamicPaging complexModel, bool applyFiltering = true) where T : class
    {
        var page = await source.AsAsyncQueryable().ToPageModel(complexModel, applyFiltering);
        return page.ToFlatModel();
    }
}