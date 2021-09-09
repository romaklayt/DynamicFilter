using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Binder.NetFramework.Mvc;
using romaklayt.DynamicFilter.Parser;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Extensions.NetFramework.Mvc
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class FilterExtensions
    {
        public static async Task<IQueryable<T>> UseFilter<T>(this IQueryable<T> source,
            ExpressionDynamicFilter<T> filter)
        {
            var result = filter.Filter != null ? source.Where(filter.Filter).AsQueryable() : source.AsQueryable();

            if (filter.Select != null)
                result = result.Select(filter.Select);

            if (filter.Order != null)
                result = filter.OrderType == OrderType.Asc
                    ? result.OrderBy(filter.Order).AsQueryable()
                    : result.OrderByDescending(filter.Order).AsQueryable();

            return await Task.FromResult(result);
        }

        public static async Task<IQueryable<T>> UseFilter<T>(this IQueryable<T> source,
            DynamicFilterModel filter)
        {
            return await source.UseFilter(filter.BindFilterExpressions<T>());
        }

        public static async Task<IEnumerable<T>> UseFilter<T>(this IEnumerable<T> source,
            ExpressionDynamicFilter<T> filter)
        {
            var result = filter.Filter != null ? source.Where(filter.Filter.Compile()) : source;

            if (filter.Select != null)
                result = result.Select(filter.Select.Compile());

            if (filter.Order != null)
                result = filter.OrderType == OrderType.Asc
                    ? result.OrderBy(filter.Order.Compile())
                    : result.OrderByDescending(filter.Order.Compile());

            return await Task.FromResult(result);
        }

        public static async Task<IEnumerable<T>> UseFilter<T>(this IEnumerable<T> source,
            DynamicFilterModel filter)
        {
            return await source.UseFilter(filter.BindFilterExpressions<T>());
        }

        public static async Task<IAsyncEnumerable<T>> UseFilter<T>(this IAsyncEnumerable<T> source,
            ExpressionDynamicFilter<T> filter)
        {
            var result = filter.Filter != null
                ? source.Where(filter.Filter.Compile()).AsAsyncEnumerable()
                : source.AsAsyncEnumerable();

            if (filter.Select != null)
                result = result.Select(filter.Select.Compile());

            if (filter.Order != null)
                result = filter.OrderType == OrderType.Asc
                    ? result.OrderBy(filter.Order.Compile()).AsAsyncEnumerable()
                    : result.OrderByDescending(filter.Order.Compile()).AsAsyncEnumerable();

            return await Task.FromResult(result);
        }

        public static async Task<IAsyncEnumerable<T>> UseFilter<T>(this IAsyncEnumerable<T> source,
            DynamicFilterModel filter)
        {
            return await source.UseFilter(filter.BindFilterExpressions<T>());
        }
    }
}