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
    public static class FilterExtensions
    {
        public static async Task<IQueryable<TTarget>> UseFilter<TSource, TTarget>(this IQueryable<TSource> source,
            ExpressionDynamicFilter<TSource, TTarget> filter, bool pagination = true)
            where TSource : class where TTarget : class
        {
            var queryable = filter.Filter != null ? source.Where(filter.Filter).AsQueryable() : source.AsQueryable();

            if (filter.Order != null)
                queryable = filter.OrderType == OrderType.Asc
                    ? queryable.OrderBy(filter.Order).AsQueryable()
                    : queryable.OrderByDescending(filter.Order).AsQueryable();
            var result = filter.Select != null ? queryable.Select(filter.Select) : queryable.Cast<TTarget>();
            if (filter.PageSize == default && filter.Page == default || !pagination)
                return await Task.FromResult(result);

            if (filter.PageSize < 1) filter.PageSize = 10;
            if (filter.Page < 1) filter.Page = 1;
            return await Task.FromResult(result.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize));
        }

        public static async Task<IQueryable<T>> UseFilter<T>(this IQueryable<T> source,
            BaseDynamicFilter filter, bool pagination = true) where T : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<T, T>(), pagination);
        }

        public static async Task<int> UseFilter<T>(this IQueryable<T> source,
            BaseCountDynamicFilter filter) where T : class
        {
            var filteredData = await source.UseFilter(filter.BindFilterExpressions<T, T>(), false);
            return filteredData.Count();
        }

        public static async Task<IQueryable<TTarget>> UseFilter<TSource, TTarget>(this IQueryable<TSource> source,
            BaseDynamicFilter filter, bool pagination = true) where TSource : class where TTarget : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<TSource, TTarget>(), pagination);
        }

        public static async Task<IEnumerable<TTarget>> UseFilter<TSource, TTarget>(this IEnumerable<TSource> source,
            ExpressionDynamicFilter<TSource, TTarget> filter, bool pagination = true)
            where TSource : class where TTarget : class
        {
            var enumerable = filter.Filter != null ? source.Where(filter.Filter.Compile()) : source;

            if (filter.Order != null)
                enumerable = filter.OrderType == OrderType.Asc
                    ? enumerable.OrderBy(filter.Order.Compile())
                    : enumerable.OrderByDescending(filter.Order.Compile());
            var result = filter.Select != null
                ? enumerable.Select(filter.Select.Compile())
                : enumerable.Cast<TTarget>();
            if (filter.PageSize == default && filter.Page == default || !pagination)
                return await Task.FromResult(result);

            if (filter.PageSize < 1) filter.PageSize = 10;
            if (filter.Page < 1) filter.Page = 1;
            return await Task.FromResult(result.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize));
        }

        public static async Task<IEnumerable<T>> UseFilter<T>(this IEnumerable<T> source,
            BaseDynamicFilter filter, bool pagination = true) where T : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<T, T>(), pagination);
        }

        public static async Task<int> UseCountFilter<T>(this IEnumerable<T> source,
            BaseCountDynamicFilter filter) where T : class
        {
            var filteredData = await source.UseFilter(filter.BindFilterExpressions<T, T>(), false);
            return filteredData.Count();
        }

        public static async Task<IEnumerable<TTarget>> UseFilter<TSource, TTarget>(this IEnumerable<TSource> source,
            BaseDynamicFilter filter, bool pagination = true) where TSource : class where TTarget : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<TSource, TTarget>(), pagination);
        }
    }
}