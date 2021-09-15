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
    public static class FilterExtensions
    {
        public static async Task<IAsyncEnumerable<TTarget>> UseFilter<TSource, TTarget>(
            this IAsyncEnumerable<TSource> source,
            ExpressionDynamicFilter<TSource, TTarget> filter) where TSource : class
            where TTarget : class
        {
            var queryable = filter.Filter != null
                ? source.Where(filter.Filter.Compile())
                : source.AsAsyncQueryable();

            if (filter.Order != null)
                queryable = filter.OrderType == OrderType.Asc
                    ? queryable.OrderBy(filter.Order.Compile())
                    : queryable.OrderByDescending(filter.Order.Compile());

            var result = filter.Select != null ? queryable.Select(filter.Select.Compile()) : queryable.Cast<TTarget>();

            return await Task.FromResult(result);
        }

        public static async Task<IAsyncEnumerable<T>> UseFilter<T>(this IAsyncEnumerable<T> source,
            BaseDynamicFilter filter) where T : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<T, T>());
        }

        public static async Task<IAsyncEnumerable<TTarget>> UseFilter<TSource, TTarget>(
            this IAsyncEnumerable<TSource> source,
            BaseDynamicFilter filter) where TSource : class where TTarget : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<TSource, TTarget>());
        }

        public static async Task<IAsyncQueryable<TTarget>> UseFilter<TSource, TTarget>(
            this IAsyncQueryable<TSource> source,
            ExpressionDynamicFilter<TSource, TTarget> filter) where TSource : class where TTarget : class
        {
            var queryable = filter.Filter != null
                ? source.Where(filter.Filter)
                : source.AsAsyncQueryable();

            if (filter.Order != null)
                queryable = filter.OrderType == OrderType.Asc
                    ? queryable.OrderBy(filter.Order)
                    : queryable.OrderByDescending(filter.Order);

            var result = filter.Select != null ? queryable.Select(filter.Select) : queryable.Cast<TTarget>();

            return await Task.FromResult(result);
        }

        public static async Task<IAsyncQueryable<TTarget>> UseFilter<TSource, TTarget>(
            this IAsyncQueryable<TSource> source,
            BaseDynamicFilter filter) where TSource : class where TTarget : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<TSource, TTarget>());
        }

        public static async Task<IAsyncQueryable<T>> UseFilter<T>(this IAsyncQueryable<T> source,
            BaseDynamicFilter filter) where T : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<T, T>());
        }
    }
}