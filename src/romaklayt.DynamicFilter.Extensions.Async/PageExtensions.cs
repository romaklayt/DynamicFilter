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
        IDynamicComplex complexModel, bool applyFiltering = true, bool applySorting = true,
        bool applySelect = true) where T : class
    {
        var filter = complexModel.BindFilterExpressions<T, T>();
        if (filter.PageSize == default) filter.PageSize = 10;
        if (filter.Page == default) filter.Page = 1;
        var filteredEntities =
            source.Apply(complexModel, applyFiltering, applySorting, false, applySelect).AsAsyncQueryable();
        var count = await filteredEntities.CountAsync();
        var items = await filteredEntities.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
            .ToListAsync();
        return await Task.FromResult(new PageModel<T>(items, count, filter.Page, filter.PageSize));
    }
}