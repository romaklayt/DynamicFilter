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
            ExpressionDynamicFilter<TSource, TTarget> filter, bool pagination = true) where TSource : class
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
            if (filter.PageSize == default && filter.Page == default || !pagination)
                return await Task.FromResult(result);

            if (filter.PageSize < 1) filter.PageSize = 10;
            if (filter.Page < 1) filter.Page = 1;
            return await Task.FromResult(result.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize));
        }

        public static async Task<IAsyncEnumerable<T>> UseFilter<T>(this IAsyncEnumerable<T> source,
            BaseDynamicFilter filter, bool pagination = true) where T : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<T, T>(), pagination);
        }

        public static async Task<IAsyncEnumerable<TTarget>> UseFilter<TSource, TTarget>(
            this IAsyncEnumerable<TSource> source,
            BaseDynamicFilter filter, bool pagination = true) where TSource : class where TTarget : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<TSource, TTarget>(), pagination);
        }

        public static async Task<IAsyncQueryable<TTarget>> UseFilter<TSource, TTarget>(
            this IAsyncQueryable<TSource> source,
            ExpressionDynamicFilter<TSource, TTarget> filter, bool pagination = true)
            where TSource : class where TTarget : class
        {
            var queryable = filter.Filter != null
                ? source.Where(filter.Filter)
                : source.AsAsyncQueryable();

            if (filter.Order != null)
                queryable = filter.OrderType == OrderType.Asc
                    ? queryable.OrderBy(filter.Order)
                    : queryable.OrderByDescending(filter.Order);

            var result = filter.Select != null ? queryable.Select(filter.Select) : queryable.Cast<TTarget>();

            if (filter.PageSize == default && filter.Page == default || !pagination)
                return await Task.FromResult(result);

            if (filter.PageSize < 1) filter.PageSize = 10;
            if (filter.Page < 1) filter.Page = 1;
            return await Task.FromResult(result.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize));
        }

        public static async Task<IAsyncQueryable<TTarget>> UseFilter<TSource, TTarget>(
            this IAsyncQueryable<TSource> source,
            BaseDynamicFilter filter, bool pagination = true) where TSource : class where TTarget : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<TSource, TTarget>(), pagination);
        }

        public static async Task<IAsyncQueryable<T>> UseFilter<T>(this IAsyncQueryable<T> source,
            BaseDynamicFilter filter, bool pagination = true) where T : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<T, T>(), pagination);
        }

        public static async Task<int> UseCountFilter<T>(this IAsyncEnumerable<T> source,
            BaseCountDynamicFilter filter) where T : class
        {
            var filteredData = await source.UseFilter(filter.BindFilterExpressions<T, T>(), false);
            return await filteredData.CountAsync();
        }

        public static async Task<int> UseCountFilter<T>(this IAsyncQueryable<T> source,
            BaseCountDynamicFilter filter) where T : class
        {
            var filteredData = await source.UseFilter(filter.BindFilterExpressions<T, T>(), false);
            return await filteredData.CountAsync();
        }
    }
}