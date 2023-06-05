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
        IDynamicComplex complexModel, bool applyFiltering = true,
        bool applySelect = true) where T : class =>
        await source.AsQueryable().ToPageModel(complexModel, applyFiltering, applySelect);

    public static async Task<PageFlatModel<T>> ToPageFlatModel<T>(this IEnumerable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true,
        bool applySelect = true) where T : class
    {
        var page = await source.AsQueryable().ToPageModel(complexModel, applyFiltering, applySelect);
        return page.ToFlatModel();
    }

    public static async Task<PageModel<T>> ToPageModel<T>(this IQueryable<T> source,
        IDynamicComplex complexModel, bool applyFiltering = true,
        bool applySelect = true) where T : class
    {
        var page = await GetPageInfo(source, complexModel);
        source = source.ApplyPaging(complexModel);
        if (applyFiltering) source = source.ApplyFilter(complexModel);
        if (applySelect) source = source.ApplySelect(complexModel);
        return new PageModel<T>(await source.ToListAsync(), page.count, page.page, page.pageSize);
    }

    public static async Task<PageModel<T>> ToPageModel<T>(this IQueryable<T> source,
        IDynamicPaging complexModel, bool applyFiltering = true) where T : class
    {
        var page = await GetPageInfo(source, complexModel);
        source = source.ApplyPaging(complexModel);
        if (applyFiltering) source = source.ApplyFilter(complexModel);
        return new PageModel<T>(await source.ToListAsync(), page.count, page.page, page.pageSize);
    }

    private static async Task<(int page, int pageSize, int count)> GetPageInfo<T>(IQueryable<T> source,
        IDynamicPaging complexModel) where T : class
    {
        var page = complexModel.BindExpressions<T, T>();
        if (page.PageSize == default) page.PageSize = 10;
        if (page.Page == default) page.Page = 1;
        var count = await source.AsNoTracking().CountAsync(complexModel);
        return (page.Page, page.PageSize, count);
    }

    public static async Task<PageModel<T>> ToPageModel<T>(this IEnumerable<T> source,
        IDynamicPaging complexModel, bool applyFiltering = true) where T : class =>
        await source.AsQueryable().ToPageModel(complexModel, applyFiltering);

    public static async Task<PageFlatModel<T>> ToPageFlatModel<T>(this IEnumerable<T> source,
        IDynamicPaging complexModel, bool applyFiltering = true) where T : class
    {
        var page = await source.AsQueryable().ToPageModel(complexModel, applyFiltering);
        return page.ToFlatModel();
    }
}