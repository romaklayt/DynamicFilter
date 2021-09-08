using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Binder.NetFramework.Mvc;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser;

namespace romaklayt.DynamicFilter.Extensions.NetFramework.Mvc
{
    public static class Extensions
    {
        public static Task<PageModel<T>> ToPagedList<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (pageSize <= decimal.Zero) pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult(new PageModel<T>(items, count, pageNumber, pageSize));
        }

        public static Task<PageModel<T>> ToPagedList<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
        {
            if (pageSize <= decimal.Zero) pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var enumerable = source.ToList();
            var count = enumerable.Count();
            var items = enumerable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult(new PageModel<T>(items, count, pageNumber, pageSize));
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IAsyncEnumerable<T> source, int pageNumber,
            int pageSize)
        {
            if (pageSize <= decimal.Zero) pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var enumerable = await source.ToListAsync();
            var count = enumerable.Count();
            var items = enumerable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PageModel<T>(items, count, pageNumber, pageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IEnumerable<T> source,
            ExpressionDynamicFilter<T> filter)
        {
            return await source.ToPagedList(filter.Page, filter.PageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IAsyncEnumerable<T> source,
            ExpressionDynamicFilter<T> filter)
        {
            return await source.ToPagedList(filter.Page, filter.PageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IQueryable<T> source,
            ExpressionDynamicFilter<T> filter)
        {
            return await source.ToPagedList(filter.Page, filter.PageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IEnumerable<T> source, DynamicFilterModel filter)
        {
            return await source.ToPagedList(filter.Page, filter.PageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IAsyncEnumerable<T> source,
            DynamicFilterModel filter)
        {
            return await source.ToPagedList(filter.Page, filter.PageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IQueryable<T> source, DynamicFilterModel filter)
        {
            return await source.ToPagedList(filter.Page, filter.PageSize);
        }

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