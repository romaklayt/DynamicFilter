using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Common.Interfaces;
using romaklayt.DynamicFilter.Parser;

namespace romaklayt.DynamicFilter.Extensions.EntityFrameworkCore;

public static class PageExtensions
{
    public static async Task<PageModel<T>> ToPageModel<T>(this IEnumerable<T> source, IDynamicComplex complexModel, bool applyFiltering = true, bool applySelect = true,
        CancellationToken cancellationToken = default)
        where T : class =>
        await source.AsQueryable().ToPageModel(complexModel, applyFiltering, applySelect, cancellationToken);

    public static async Task<PageFlatModel<T>> ToPageFlatModel<T>(this IEnumerable<T> source, IDynamicComplex complexModel, bool applyFiltering = true, bool applySelect = true)
        where T : class
    {
        var page = await source.AsQueryable().ToPageModel(complexModel, applyFiltering, applySelect);
        return page.ToFlatModel();
    }

    public static async Task<PageModel<T>> ToPageModel<T>(this IQueryable<T> source, IDynamicComplex complexModel, bool applyFiltering = true, bool applySelect = true,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var page = await GetPageInfo(source, complexModel, cancellationToken);
        if (applyFiltering) source = source.ApplyFilter(complexModel);
        source = source.ApplyPaging(complexModel);
        if (applySelect) source = source.ApplySelect(complexModel);
        return new PageModel<T>(await source.ToListAsync(cancellationToken), page.count, page.page, page.pageSize);
    }

    public static async Task<PageModel<T>> ToPageModel<T>(this IQueryable<T> source, IDynamicPaging complexModel, bool applyFiltering = true,
        CancellationToken cancellationToken = default) where T : class
    {
        var page = await GetPageInfo(source, complexModel, cancellationToken);
        if (applyFiltering) source = source.ApplyFilter(complexModel);
        source = source.ApplyPaging(complexModel);
        return new PageModel<T>(await source.ToListAsync(cancellationToken), page.count, page.page, page.pageSize);
    }

    private static async Task<(int page, int pageSize, int count)> GetPageInfo<T>(IQueryable<T> source, IDynamicPaging complexModel, CancellationToken cancellationToken = default)
        where T : class
    {
        var page = complexModel.BindExpressions<T, T>();
        if (page.PageSize == default) page.PageSize = 10;
        if (page.Page == default) page.Page = 1;
        var count = await source.AsNoTracking().CountAsync(complexModel, cancellationToken);
        return (page.Page, page.PageSize, count);
    }

    public static async Task<PageModel<T>> ToPageModel<T>(this IEnumerable<T> source, IDynamicPaging complexModel, bool applyFiltering = true,
        CancellationToken cancellationToken = default) where T : class =>
        await source.AsQueryable().ToPageModel(complexModel, applyFiltering, cancellationToken);

    public static async Task<PageFlatModel<T>> ToPageFlatModel<T>(this IEnumerable<T> source, IDynamicPaging complexModel, bool applyFiltering = true,
        CancellationToken cancellationToken = default) where T : class
    {
        var page = await source.AsQueryable().ToPageModel(complexModel, applyFiltering, cancellationToken);
        return page.ToFlatModel();
    }
}