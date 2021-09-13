using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Extensions.EntityFramework
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class FilterExtensions
    {
        public static async Task<IQueryable<T>> UseFilter<T>(this DbSet<T> source,
            ExpressionDynamicFilter<T> filter) where T : class
        {
            var result = filter.Filter != null ? source.Where(filter.Filter).AsQueryable() : source.AsQueryable();
            if (filter.Select != null)
                result = result.Select(filter.Select);

            if (filter.Order != null)
                result = filter.OrderType == OrderType.Asc
                    ? result.OrderBy(filter.Order).AsQueryable()
                    : result.OrderByDescending(filter.Order).AsQueryable();

            if (filter.Include != null) result = result.Include(filter.Include);
            if (filter.AsNoTracking) result = result.AsNoTracking();
            return await Task.FromResult(result);
        }

        public static async Task<IQueryable<T>> UseFilter<T>(this DbSet<T> source,
            BaseDynamicFilter filter) where T : class
        {
            return await source.UseFilter(filter.BindFilterExpressions<T>());
        }
    }
}