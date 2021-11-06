using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace romaklayt.DynamicFilter.Extensions.Async
{
    public static class LinqDynamicExtensions
    {
        public static async Task<TEntity> DynamicFirstOfDefaultAsync<TEntity, TKeyValue>(
            IAsyncQueryable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await source.FirstOrDefaultAsync(GenerateConstantExpression(source, propertyName, keyValue));
        }

        public static async Task<TEntity> DynamicFirstOfDefaultAsync<TEntity, TKeyValue>(
            IAsyncEnumerable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await DynamicFirstOfDefaultAsync(source.AsAsyncQueryable(), propertyName, keyValue);
        }

        public static async Task<TEntity> DynamicFirstAsync<TEntity, TKeyValue>(IAsyncQueryable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await source.FirstAsync(GenerateConstantExpression(source, propertyName, keyValue));
        }

        public static async Task<TEntity> DynamicFirstAsync<TEntity, TKeyValue>(IAsyncEnumerable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await DynamicFirstOfDefaultAsync(source.AsAsyncQueryable(), propertyName, keyValue);
        }

        public static async Task<TEntity> DynamicLastOfDefaultAsync<TEntity, TKeyValue>(IAsyncQueryable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await source.LastOrDefaultAsync(GenerateConstantExpression(source, propertyName, keyValue));
        }

        public static async Task<TEntity> DynamicLastOfDefaultAsync<TEntity, TKeyValue>(
            IAsyncEnumerable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await DynamicLastOfDefaultAsync(source.AsAsyncQueryable(), propertyName, keyValue);
        }

        public static async Task<TEntity> DynamicLastAsync<TEntity, TKeyValue>(IAsyncQueryable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await source.LastAsync(GenerateConstantExpression(source, propertyName, keyValue));
        }

        public static async Task<TEntity> DynamicLastAsync<TEntity, TKeyValue>(IAsyncEnumerable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await DynamicLastOfDefaultAsync(source.AsAsyncQueryable(), propertyName, keyValue);
        }

        public static IAsyncQueryable<TEntity> DynamicOrderBy<TEntity>(this IAsyncQueryable<TEntity> source,
            string orderByProperty)
        {
            return source.GenerateOrderExpression(orderByProperty, false);
        }

        public static IAsyncQueryable<TEntity> DynamicOrderByDescending<TEntity>(this IAsyncQueryable<TEntity> source,
            string orderByProperty)
        {
            return source.GenerateOrderExpression(orderByProperty, true);
        }

        public static IAsyncQueryable<TEntity> DynamicOrderBy<TEntity>(this IAsyncEnumerable<TEntity> source,
            string orderByProperty)
        {
            return source.AsAsyncQueryable().GenerateOrderExpression(orderByProperty, false);
        }

        public static IAsyncQueryable<TEntity> DynamicOrderByDescending<TEntity>(this IAsyncEnumerable<TEntity> source,
            string orderByProperty)
        {
            return source.AsAsyncQueryable().GenerateOrderExpression(orderByProperty, true);
        }

        private static IAsyncQueryable<TEntity> GenerateOrderExpression<TEntity>(this IAsyncQueryable<TEntity> source,
            string orderByProperty, bool desc)
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
            var equal = Expression.Condition(Expression.Equal(property, Expression.Constant(null, property.Type)),
                Expression.Constant(null, property.Type), Expression.Equal(property, Expression.Constant(keyValue)));
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equal, parameter);
            return lambda;
        }
    }
}