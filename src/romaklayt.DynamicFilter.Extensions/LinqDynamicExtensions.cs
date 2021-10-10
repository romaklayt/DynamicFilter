using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace romaklayt.DynamicFilter.Extensions
{
    public static class LinqDynamicExtensions
    {
        public static async Task<TEntity> FirstOfDefault<TEntity, TKeyValue>(
            IQueryable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await Task.FromResult(
                source.FirstOrDefault(GenerateConstantExpression(source, propertyName, keyValue)));
        }

        public static async Task<TEntity> FirstOfDefault<TEntity, TKeyValue>(
            IEnumerable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await FirstOfDefault(source.AsQueryable(), propertyName, keyValue);
        }

        public static async Task<TEntity> First<TEntity, TKeyValue>(
            IQueryable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await Task.FromResult(source.First(GenerateConstantExpression(source, propertyName, keyValue)));
        }

        public static async Task<TEntity> First<TEntity, TKeyValue>(
            IEnumerable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await FirstOfDefault(source.AsQueryable(), propertyName, keyValue);
        }

        public static async Task<TEntity> LastOfDefault<TEntity, TKeyValue>(
            IQueryable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await Task.FromResult(
                source.LastOrDefault(GenerateConstantExpression(source, propertyName, keyValue)));
        }

        public static async Task<TEntity> LastOfDefault<TEntity, TKeyValue>(
            IEnumerable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await LastOfDefault(source.AsQueryable(), propertyName, keyValue);
        }

        public static async Task<TEntity> Last<TEntity, TKeyValue>(
            IQueryable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await Task.FromResult(source.Last(GenerateConstantExpression(source, propertyName, keyValue)));
        }

        public static async Task<TEntity> Last<TEntity, TKeyValue>(
            IEnumerable<TEntity> source, string propertyName, TKeyValue keyValue)
        {
            return await LastOfDefault(source.AsQueryable(), propertyName, keyValue);
        }

        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string orderByProperty)
        {
            return source.GenerateOrderExpression(orderByProperty, false);
        }

        public static IQueryable<TEntity> OrderByDescending<TEntity>(this IQueryable<TEntity> source,
            string orderByProperty)
        {
            return source.GenerateOrderExpression(orderByProperty, true);
        }

        public static IQueryable<TEntity> OrderBy<TEntity>(this IEnumerable<TEntity> source, string orderByProperty)
        {
            return source.AsQueryable().GenerateOrderExpression(orderByProperty, false);
        }

        public static IQueryable<TEntity> OrderByDescending<TEntity>(this IEnumerable<TEntity> source,
            string orderByProperty)
        {
            return source.AsQueryable().GenerateOrderExpression(orderByProperty, true);
        }

        private static IQueryable<TEntity> GenerateOrderExpression<TEntity>(this IQueryable<TEntity> source,
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