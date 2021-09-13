using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Extensions.Async
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class PageExtensions
    {
        public static async Task<PageModel<T>> ToPagedList<T>(this IAsyncEnumerable<T> source, int pageNumber,
            int pageSize) where T : class
        {
            if (pageSize < 1) pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var enumerable = await source.ToListAsync();
            var count = enumerable.Count();
            var items = enumerable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PageModel<T>(items, count, pageNumber, pageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IAsyncEnumerable<T> source,
            ExpressionDynamicFilter<T> filter) where T : class
        {
            return await source.ToPagedList(filter.Page, filter.PageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IAsyncEnumerable<T> source,
            BaseDynamicFilter filter) where T : class
        {
            return await source.ToPagedList(filter.Page, filter.PageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IAsyncQueryable<T> source, int pageNumber,
            int pageSize) where T : class
        {
            if (pageSize < 1) pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PageModel<T>(items, count, pageNumber, pageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IAsyncQueryable<T> source,
            ExpressionDynamicFilter<T> filter) where T : class
        {
            return await source.ToPagedList(filter.Page, filter.PageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IAsyncQueryable<T> source,
            BaseDynamicFilter filter) where T : class
        {
            return await source.ToPagedList(filter.Page, filter.PageSize);
        }
    }
}