using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Binder;
using romaklayt.DynamicFilter.Parser;

namespace romaklayt.DynamicFilter.Extensions
{
    public static class Extensions
    {
        private static Task<PageModel<T>> ToPagedList<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (pageSize <= decimal.Zero) pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult(new PageModel<T>(items, count, pageNumber, pageSize));
        }

        private static Task<PageModel<T>> ToPagedList<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
        {
            if (pageSize <= decimal.Zero) pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var enumerable = source.ToList();
            var count = enumerable.Count();
            var items = enumerable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult(new PageModel<T>(items, count, pageNumber, pageSize));
        }

        private static async Task<PageModel<T>> ToPagedList<T>(this IAsyncEnumerable<T> source, int pageNumber,
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
            IQueryable<T> result;
            if (filter.Filter != null)
                result = source.Where(filter.Filter).AsQueryable();
            else
                result = source.AsQueryable();

            if (filter.Select != null)
                result = result.Select(filter.Select);

            if (filter.Order != null)
            {
                if (filter.OrderType == OrderType.Asc)
                    result = result.OrderBy(filter.Order).AsQueryable();
                else
                    result = result.OrderByDescending(filter.Order).AsQueryable();
            }

            return result;
        }

        public static async Task<IQueryable<T>> UseFilter<T>(this IQueryable<T> source,
            DynamicFilterModel filter)
        {
            return await source.UseFilter(filter.BindFilterExpressions<T>());
        }

        public static async Task<IEnumerable<T>> UseFilter<T>(this IEnumerable<T> source,
            ExpressionDynamicFilter<T> filter)
        {
            IEnumerable<T> result;
            if (filter.Filter != null)
                result = source.Where(filter.Filter.Compile());
            else
                result = source;

            if (filter.Select != null)
                result = result.Select(filter.Select.Compile());

            if (filter.Order != null)
            {
                if (filter.OrderType == OrderType.Asc)
                    result = result.OrderBy(filter.Order.Compile());
                else
                    result = result.OrderByDescending(filter.Order.Compile());
            }

            return result;
        }

        public static async Task<IEnumerable<T>> UseFilter<T>(this IEnumerable<T> source,
            DynamicFilterModel filter)
        {
            return await source.UseFilter(filter.BindFilterExpressions<T>());
        }

        public static async Task<IAsyncEnumerable<T>> UseFilter<T>(this IAsyncEnumerable<T> source,
            ExpressionDynamicFilter<T> filter)
        {
            IAsyncEnumerable<T> result;
            if (filter.Filter != null)
                result = source.Where(filter.Filter.Compile()).AsAsyncEnumerable();
            else
                result = source.AsAsyncEnumerable();

            if (filter.Select != null)
                result = result.Select(filter.Select.Compile());

            if (filter.Order != null)
            {
                if (filter.OrderType == OrderType.Asc)
                    result = result.OrderBy(filter.Order.Compile()).AsAsyncEnumerable();
                else
                    result = result.OrderByDescending(filter.Order.Compile()).AsAsyncEnumerable();
            }

            return result;
        }

        public static async Task<IAsyncEnumerable<T>> UseFilter<T>(this IAsyncEnumerable<T> source,
            DynamicFilterModel filter)
        {
            return await source.UseFilter(filter.BindFilterExpressions<T>());
        }
    }
}