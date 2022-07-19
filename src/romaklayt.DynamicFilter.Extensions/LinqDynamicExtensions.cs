using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
// ReSharper disable MemberCanBePrivate.Global

namespace romaklayt.DynamicFilter.Extensions;

public static class LinqDynamicExtensions
{
    public static async Task<TEntity> DynamicFirstOfDefault<TEntity, TKeyValue>(this IQueryable<TEntity> source,
        string propertyName, TKeyValue keyValue)
    {
        return await Task.FromResult(source.FirstOrDefault(GenerateConstantExpression(source, propertyName, keyValue)));
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
        return await Task.FromResult(source.LastOrDefault(GenerateConstantExpression(source, propertyName, keyValue)));
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
        params Tuple<string, bool>[] order)
    {
        IOrderedQueryable<TEntity> orderExpression = null;
        for (var index = 0; index < order.Length; index++)
        {
            var tuple = order[index];
            if (index == 0)
                orderExpression = tuple.Item2
                    ? source.DynamicOrderByMemberDescending(tuple.Item1)
                    : source.DynamicOrderByMember(tuple.Item1);
            else
                orderExpression = tuple.Item2
                    ? orderExpression.DynamicThenByMemberDescending(tuple.Item1)
                    : orderExpression.DynamicThenByMember(tuple.Item1);
        }

        return orderExpression ?? source;
    }

    public static IOrderedQueryable<T> DynamicOrderByMember<T>(this IQueryable<T> source, string memberPath)
    {
        return source.OrderByMemberUsing(memberPath, "OrderBy");
    }

    public static IOrderedQueryable<T> DynamicOrderByMemberDescending<T>(this IQueryable<T> source, string memberPath)
    {
        return source.OrderByMemberUsing(memberPath, "OrderByDescending");
    }

    public static IOrderedQueryable<T> DynamicThenByMember<T>(this IOrderedQueryable<T> source, string memberPath)
    {
        return source.OrderByMemberUsing(memberPath, "ThenBy");
    }

    public static IOrderedQueryable<T> DynamicThenByMemberDescending<T>(this IOrderedQueryable<T> source,
        string memberPath)
    {
        return source.OrderByMemberUsing(memberPath, "ThenByDescending");
    }

    public static IOrderedQueryable<T> DynamicOrderByMember<T>(this IEnumerable<T> source, string memberPath)
    {
        return source.AsQueryable().OrderByMemberUsing(memberPath, "OrderBy");
    }

    public static IOrderedQueryable<T> DynamicOrderByMemberDescending<T>(this IEnumerable<T> source, string memberPath)
    {
        return source.AsQueryable().OrderByMemberUsing(memberPath, "OrderByDescending");
    }

    private static IOrderedQueryable<T> OrderByMemberUsing<T>(this IQueryable<T> source, string memberPath,
        string method)
    {
        var parameter = Expression.Parameter(typeof(T), "item");
        var member = memberPath.Split('.').Aggregate((Expression)parameter, Expression.PropertyOrField);
        var keySelector = Expression.Lambda(member, parameter);
        var methodCall = Expression.Call(typeof(Queryable), method, new[] { parameter.Type, member.Type },
            source.Expression, Expression.Quote(keySelector));
        return (IOrderedQueryable<T>)source.Provider.CreateQuery(methodCall);
    }

    private static Expression<Func<TEntity, bool>> GenerateConstantExpression<TEntity, TKeyValue>(
        IQueryable<TEntity> source, string propertyName, TKeyValue keyValue)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var property = Expression.PropertyOrField(parameter, propertyName);
        var equal = Expression.Equal(property, Expression.Constant(keyValue));
        var lambda = Expression.Lambda<Func<TEntity, bool>>(equal, parameter);
        return lambda;
    }
}