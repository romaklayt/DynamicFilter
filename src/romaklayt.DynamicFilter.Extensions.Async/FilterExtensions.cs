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
            BaseDynamicFilter filter)
        {
            return await source.UseFilter(filter.BindFilterExpressions<T>());
        }

        public static async Task<IAsyncQueryable<T>> UseFilter<T>(this IAsyncQueryable<T> source,
            ExpressionDynamicFilter<T> filter)
        {
            var result = filter.Filter != null
                ? source.Where(filter.Filter).AsAsyncQueryable()
                : source.AsAsyncQueryable();

            if (filter.Select != null)
                result = result.Select(filter.Select);

            if (filter.Order != null)
                result = filter.OrderType == OrderType.Asc
                    ? result.OrderBy(filter.Order.Compile()).AsAsyncQueryable()
                    : result.OrderByDescending(filter.Order.Compile()).AsAsyncQueryable();

            return await Task.FromResult(result);
        }

        public static async Task<IAsyncQueryable<T>> UseFilter<T>(this IAsyncQueryable<T> source,
            BaseDynamicFilter filter)
        {
            return await source.UseFilter(filter.BindFilterExpressions<T>());
        }
    }
}