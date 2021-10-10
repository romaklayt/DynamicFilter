using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace romaklayt.DynamicFilter.Extensions.Async
{
    public static class LinqDynamicExtensions
    {
        public static async Task<TEntity> FirstOfDefaultAsync<TEntity, TKeyValue>(
            IAsyncQueryable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await source.FirstOrDefaultAsync(GenerateConstantExpression(source, propertyName, keyValue));
        }

        public static async Task<TEntity> FirstOfDefaultAsync<TEntity, TKeyValue>(
            IAsyncEnumerable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await FirstOfDefaultAsync(source.AsAsyncQueryable(), propertyName, keyValue);
        }

        public static async Task<TEntity> FirstAsync<TEntity, TKeyValue>(
            IAsyncQueryable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await source.FirstAsync(GenerateConstantExpression(source, propertyName, keyValue));
        }

        public static async Task<TEntity> FirstAsync<TEntity, TKeyValue>(
            IAsyncEnumerable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await FirstOfDefaultAsync(source.AsAsyncQueryable(), propertyName, keyValue);
        }

        public static async Task<TEntity> LastOfDefaultAsync<TEntity, TKeyValue>(
            IAsyncQueryable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await source.LastOrDefaultAsync(GenerateConstantExpression(source, propertyName, keyValue));
        }

        public static async Task<TEntity> LastOfDefaultAsync<TEntity, TKeyValue>(
            IAsyncEnumerable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await LastOfDefaultAsync(source.AsAsyncQueryable(), propertyName, keyValue);
        }

        public static async Task<TEntity> LastAsync<TEntity, TKeyValue>(
            IAsyncQueryable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await source.LastAsync(GenerateConstantExpression(source, propertyName, keyValue));
        }

        public static async Task<TEntity> LastAsync<TEntity, TKeyValue>(
            IAsyncEnumerable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await LastOfDefaultAsync(source.AsAsyncQueryable(), propertyName, keyValue);
        }

        public static IAsyncQueryable<TEntity> OrderBy<TEntity>(this IAsyncQueryable<TEntity> source,
            string orderByProperty)
        {
            return source.GenerateOrderExpression(orderByProperty, false);
        }

        public static IAsyncQueryable<TEntity> OrderByDescending<TEntity>(this IAsyncQueryable<TEntity> source,
            string orderByProperty)
        {
            return source.GenerateOrderExpression(orderByProperty, true);
        }

        public static IAsyncQueryable<TEntity> OrderBy<TEntity>(this IAsyncEnumerable<TEntity> source,
            string orderByProperty)
        {
            return source.AsAsyncQueryable().GenerateOrderExpression(orderByProperty, false);
        }

        public static IAsyncQueryable<TEntity> OrderByDescending<TEntity>(this IAsyncEnumerable<TEntity> source,
            string orderByProperty)
        {
            return source.AsAsyncQueryable().GenerateOrderExpression(orderByProperty, true);
        }

        private static IAsyncQueryable<TEntity> GenerateOrderExpression<TEntity>(this IAsyncQueryable<TEntity> source,
            string orderByProperty,
            bool desc)
        {
            var command = desc ? "OrderByDescending" : "OrderBy";
            var type = typeof(TEntity);
            var property = type.GetProperty(orderByProperty);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new[] {type, property.PropertyType},
                source.Expression, Expression.Quote(orderByExpression));
            return source.Provider.CreateQuery<TEntity>(resultExpression);
        }

        private static Expression<Func<TEntity, bool>> GenerateConstantExpression<TEntity, TKeyValue>(
            IAsyncQueryable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = Expression.Property(parameter, propertyName);
            var equal = Expression.Equal(property, Expression.Constant(keyValue));
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equal, parameter);
            return lambda;
        }
    }
}