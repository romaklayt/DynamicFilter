using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Extensions
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class PageExtensions
    {
        public static async Task<PageModel<TTarget>> ToPagedList<TSource, TTarget>(this IQueryable<TSource> source,
            ExpressionDynamicFilter<TSource, TTarget> filter) where TSource : class where TTarget : class
        {
            if (filter.PageSize < 1) filter.PageSize = 10;
            if (filter.Page < 1) filter.Page = 1;
            var filteredEntities = await source.UseFilter(filter, false);
            var count = filteredEntities.Count();
            var items = filteredEntities.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();
            return await Task.FromResult(new PageModel<TTarget>(items, count, filter.Page, filter.PageSize));
        }

        public static async Task<PageModel<TTarget>> ToPagedList<TSource, TTarget>(this IEnumerable<TSource> source,
            ExpressionDynamicFilter<TSource, TTarget> filter) where TSource : class where TTarget : class
        {
            if (filter.PageSize < 1) filter.PageSize = 10;
            if (filter.Page < 1) filter.Page = 1;
            var filteredEntities = await source.UseFilter(filter, false);
            var enumerable = filteredEntities.ToList();
            var count = enumerable.Count();
            var items = enumerable.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();
            return await Task.FromResult(new PageModel<TTarget>(items, count, filter.Page, filter.PageSize));
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IEnumerable<T> source, BaseDynamicFilter filter)
            where T : class
        {
            return await source.ToPagedList(filter.BindFilterExpressions<T, T>());
        }


        public static async Task<PageModel<T>> ToPagedList<T>(this IQueryable<T> source, BaseDynamicFilter filter)
            where T : class
        {
            return await source.ToPagedList(filter.BindFilterExpressions<T, T>());
        }

        public static async Task<PageModel<TTarget>> ToPagedList<TSource, TTarget>(this IEnumerable<TSource> source,
            BaseDynamicFilter filter)
            where TSource : class where TTarget : class
        {
            return await source.ToPagedList(filter.BindFilterExpressions<TSource, TTarget>());
        }


        public static async Task<PageModel<TTarget>> ToPagedList<TSource, TTarget>(this IQueryable<TSource> source,
            BaseDynamicFilter filter)
            where TSource : class where TTarget : class
        {
            return await source.ToPagedList(filter.BindFilterExpressions<TSource, TTarget>());
        }
    }
}