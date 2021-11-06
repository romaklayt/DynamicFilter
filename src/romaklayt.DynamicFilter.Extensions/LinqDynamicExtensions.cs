using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace romaklayt.DynamicFilter.Extensions
{
    public static class LinqDynamicExtensions
    {
        public static async Task<TEntity> DynamicFirstOfDefault<TEntity, TKeyValue>(this IQueryable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await Task.FromResult(
                source.FirstOrDefault(GenerateConstantExpression(source, propertyName, keyValue)));
        }

        public static async Task<TEntity> DynamicFirstOfDefault<TEntity, TKeyValue>(this IEnumerable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await DynamicFirstOfDefault(source.AsQueryable(), propertyName, keyValue);
        }

        public static async Task<TEntity> DynamicFirst<TEntity, TKeyValue>(this IQueryable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await Task.FromResult(source.First(GenerateConstantExpression(source, propertyName, keyValue)));
        }

        public static async Task<TEntity> DynamicFirst<TEntity, TKeyValue>(this IEnumerable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await DynamicFirstOfDefault(source.AsQueryable(), propertyName, keyValue);
        }

        public static async Task<TEntity> DynamicLastOfDefault<TEntity, TKeyValue>(this IQueryable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await Task.FromResult(
                source.LastOrDefault(GenerateConstantExpression(source, propertyName, keyValue)));
        }

        public static async Task<TEntity> DynamicLastOfDefault<TEntity, TKeyValue>(this IEnumerable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await DynamicLastOfDefault(source.AsQueryable(), propertyName, keyValue);
        }

        public static async Task<TEntity> DynamicLast<TEntity, TKeyValue>(this IQueryable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await Task.FromResult(source.Last(GenerateConstantExpression(source, propertyName, keyValue)));
        }

        public static async Task<TEntity> DynamicLast<TEntity, TKeyValue>(this IEnumerable<TEntity> source,
            string propertyName, TKeyValue keyValue)
        {
            return await DynamicLastOfDefault(source.AsQueryable(), propertyName, keyValue);
        }

        public static IQueryable<TEntity> DynamicOrderBy<TEntity>(this IQueryable<TEntity> source,
            string orderByProperty)
        {
            return source.GenerateOrderExpression(orderByProperty, false);
        }

        public static IQueryable<TEntity> DynamicOrderByDescending<TEntity>(this IQueryable<TEntity> source,
            string orderByProperty)
        {
            return source.GenerateOrderExpression(orderByProperty, true);
        }

        public static IQueryable<TEntity> DynamicOrderBy<TEntity>(this IEnumerable<TEntity> source,
            string orderByProperty)
        {
            return source.AsQueryable().GenerateOrderExpression(orderByProperty, false);
        }

        public static IQueryable<TEntity> DynamicOrderByDescending<TEntity>(this IEnumerable<TEntity> source,
            string orderByProperty)
        {
            return source.AsQueryable().GenerateOrderExpression(orderByProperty, true);
        }

        private static IQueryable<TEntity> GenerateOrderExpression<TEntity>(this IQueryable<TEntity> source,
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
            IQueryable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = Expression.Property(parameter, propertyName);
            var equal = Expression.Equal(property, Expression.Constant(keyValue));
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equal, parameter);
            return lambda;
        }
    }
}