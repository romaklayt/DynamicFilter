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
        public static async Task<IQueryable<T>> UseFilter<T>(this IQueryable<T> source,
            ExpressionDynamicFilter<T> filter) where T : class
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
            BaseDynamicFilter filter) where T : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<T>());
        }

        public static async Task<IEnumerable<T>> UseFilter<T>(this IEnumerable<T> source,
            ExpressionDynamicFilter<T> filter) where T : class
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
            BaseDynamicFilter filter) where T : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<T>());
        }
    }
}