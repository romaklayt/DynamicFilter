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
        public static async Task<IAsyncEnumerable<T>> UseFilter<T>(this IAsyncEnumerable<T> source,
            BaseDynamicComplexModel complexModel, bool pagination = true) where T : class
        {
            return await source.AsAsyncQueryable().UseFilter(complexModel.BindFilterExpressions<T, T>(), pagination);
        }

        public static async Task<IAsyncEnumerable<TTarget>> UseFilter<TSource, TTarget>(
            this IAsyncEnumerable<TSource> source,
            BaseDynamicComplexModel complexModel, bool pagination = true) where TSource : class where TTarget : class
        {
            return await source.AsAsyncQueryable()
                .UseFilter(complexModel.BindFilterExpressions<TSource, TTarget>(), pagination);
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
            BaseDynamicComplexModel complexModel, bool pagination = true) where TSource : class where TTarget : class
        {
            return await source.UseFilter(complexModel.BindFilterExpressions<TSource, TTarget>(), pagination);
        }

        public static async Task<IAsyncQueryable<T>> UseFilter<T>(this IAsyncQueryable<T> source,
            BaseDynamicComplexModel complexModel, bool pagination = true) where T : class
        {
            return await source.UseFilter(complexModel.BindFilterExpressions<T, T>(), pagination);
        }

        public static async Task<IAsyncEnumerable<T>> UseFilter<T>(this IAsyncEnumerable<T> source,
            BaseDynamicFilterModel filterModel) where T : class
        {
            var filteredData =
                await source.AsAsyncQueryable().UseFilter(filterModel.BindFilterExpressions<T, T>(), false);
            return filteredData;
        }

        public static async Task<IAsyncQueryable<T>> UseFilter<T>(this IAsyncQueryable<T> source,
            BaseDynamicFilterModel filterModel) where T : class
        {
            var filteredData = await source.UseFilter(filterModel.BindFilterExpressions<T, T>(), false);
            return filteredData;
        }

        public static async Task<IAsyncEnumerable<T>> UseSelect<T>(this IAsyncEnumerable<T> source,
            BaseDynamicSelectModel selectModel) where T : class
        {
            var result = selectModel.Select != null
                ? source.Select(selectModel.BindFilterExpressions<T, T>().Select.Compile())
                : source;
            return await Task.FromResult(result);
        }

        public static async Task<IAsyncQueryable<T>> UseSelect<T>(this IAsyncQueryable<T> source,
            BaseDynamicSelectModel selectModel) where T : class
        {
            var result = selectModel.Select != null
                ? source.Select(selectModel.BindFilterExpressions<T, T>().Select)
                : source;
            return await Task.FromResult(result);
        }
    }
}