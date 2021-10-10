using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Extensions.Async
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class PageExtensions
    {
        public static async Task<PageModel<TTarget>> ToPagedList<TSource, TTarget>(
            this IAsyncEnumerable<TSource> source, ExpressionDynamicFilter<TSource, TTarget> filter)
            where TSource : class where TTarget : class
        {
            if (filter.PageSize < 1) filter.PageSize = 10;
            if (filter.Page < 1) filter.Page = 1;
            var filteredEntities = await source.AsAsyncQueryable().UseFilter(filter, false);
            var enumerable = await filteredEntities.ToListAsync();
            var count = enumerable.Count();
            var items = enumerable.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();
            return new PageModel<TTarget>(items, count, filter.Page, filter.PageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IAsyncEnumerable<T> source,
            BaseDynamicComplexModel complexModel) where T : class
        {
            return await source.ToPagedList(complexModel.BindFilterExpressions<T, T>());
        }

        public static async Task<PageModel<TTarget>> ToPagedList<TSource, TTarget>(
            this IAsyncEnumerable<TSource> source, BaseDynamicComplexModel complexModel)
            where TSource : class where TTarget : class
        {
            return await source.ToPagedList(complexModel.BindFilterExpressions<TSource, TTarget>());
        }

        public static async Task<PageModel<TTarget>> ToPagedList<TSource, TTarget>(this IAsyncQueryable<TSource> source,
            ExpressionDynamicFilter<TSource, TTarget> filter) where TSource : class where TTarget : class
        {
            if (filter.PageSize < 1) filter.PageSize = 10;
            if (filter.Page < 1) filter.Page = 1;
            var filteredEntities = await source.UseFilter(filter, false);
            var count = await filteredEntities.CountAsync();
            var items = await filteredEntities.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
                .ToListAsync();
            return new PageModel<TTarget>(items, count, filter.Page, filter.PageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IAsyncQueryable<T> source,
            BaseDynamicComplexModel complexModel) where T : class
        {
            return await source.ToPagedList(complexModel.BindFilterExpressions<T, T>());
        }

        public static async Task<PageModel<TTarget>> ToPagedList<TSource, TTarget>(this IAsyncQueryable<TSource> source,
            BaseDynamicComplexModel complexModel) where TSource : class where TTarget : class
        {
            return await source.ToPagedList(complexModel.BindFilterExpressions<TSource, TTarget>());
        }
    }
}