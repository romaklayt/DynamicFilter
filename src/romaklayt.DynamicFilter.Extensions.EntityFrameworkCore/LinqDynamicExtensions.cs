﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Extensions.EntityFrameworkCore;

public static class LinqDynamicExtensions
{
    public static async Task<int> CountAsync<TEntity>(this IQueryable<TEntity> source, IDynamicFilter dynamicFilter, CancellationToken cancellationToken = default)
        where TEntity : class =>
        await source.ApplyFilter(dynamicFilter, false).CountAsync(cancellationToken);

    public static async Task<int> CountAsync<TEntity>(this IEnumerable<TEntity> source, IDynamicFilter dynamicFilter, CancellationToken cancellationToken = default)
        where TEntity : class =>
        await source.AsQueryable().CountAsync(dynamicFilter, cancellationToken);

    public static async Task<TEntity> DynamicFirstOfDefault<TEntity, TKeyValue>(this IQueryable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default) where TEntity : class =>
        await source.FirstOrDefaultAsync(GenerateConstantExpression<TEntity, TKeyValue>(propertyName, keyValue), cancellationToken);

    public static async Task<TEntity> DynamicFirstOfDefault<TEntity, TKeyValue>(this IEnumerable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default) where TEntity : class =>
        await DynamicFirstOfDefault(source.AsQueryable(), propertyName, keyValue, cancellationToken);

    public static async Task<TEntity> DynamicFirst<TEntity, TKeyValue>(this IQueryable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default) where TEntity : class =>
        await source.FirstAsync(GenerateConstantExpression<TEntity, TKeyValue>(propertyName, keyValue), cancellationToken);

    public static async Task<TEntity> DynamicFirst<TEntity, TKeyValue>(this IEnumerable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default) where TEntity : class =>
        await DynamicFirstOfDefault(source.AsQueryable(), propertyName, keyValue, cancellationToken);

    public static async Task<TEntity> DynamicLastOfDefault<TEntity, TKeyValue>(this IQueryable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default) where TEntity : class =>
        await source.LastOrDefaultAsync(GenerateConstantExpression<TEntity, TKeyValue>(propertyName, keyValue), cancellationToken);

    public static async Task<TEntity> DynamicLastOfDefault<TEntity, TKeyValue>(this IEnumerable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default) where TEntity : class =>
        await DynamicLastOfDefault(source.AsQueryable(), propertyName, keyValue, cancellationToken);

    public static async Task<TEntity> DynamicLast<TEntity, TKeyValue>(this IQueryable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default) where TEntity : class =>
        await source.LastAsync(GenerateConstantExpression<TEntity, TKeyValue>(propertyName, keyValue), cancellationToken);

    public static async Task<TEntity> DynamicLast<TEntity, TKeyValue>(this IEnumerable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default) where TEntity : class =>
        await DynamicLastOfDefault(source.AsQueryable(), propertyName, keyValue, cancellationToken);

    public static IQueryable<TEntity> DynamicOrderBy<TEntity>(this IQueryable<TEntity> source, params Tuple<string, bool>[] order)
    {
        IOrderedQueryable<TEntity> orderExpression = null;
        for (var index = 0; index < order.Length; index++)
        {
            var tuple = order[index];
            if (index == 0)
                orderExpression = tuple.Item2 ? source.DynamicOrderByMemberDescending(tuple.Item1) : source.DynamicOrderByMember(tuple.Item1);
            else
                orderExpression = tuple.Item2 ? orderExpression.DynamicThenByMemberDescending(tuple.Item1) : orderExpression.DynamicThenByMember(tuple.Item1);
        }

        return orderExpression ?? source;
    }

    public static IOrderedQueryable<T> DynamicOrderByMember<T>(this IQueryable<T> source, string memberPath) => source.OrderByMemberUsing(memberPath, "OrderBy");

    public static IOrderedQueryable<T> DynamicOrderByMemberDescending<T>(this IQueryable<T> source, string memberPath) =>
        source.OrderByMemberUsing(memberPath, "OrderByDescending");

    public static IOrderedQueryable<T> DynamicThenByMember<T>(this IOrderedQueryable<T> source, string memberPath) => source.OrderByMemberUsing(memberPath, "ThenBy");

    public static IOrderedQueryable<T> DynamicThenByMemberDescending<T>(this IOrderedQueryable<T> source, string memberPath) =>
        source.OrderByMemberUsing(memberPath, "ThenByDescending");

    public static IOrderedQueryable<T> DynamicOrderByMember<T>(this IEnumerable<T> source, string memberPath) => source.AsQueryable().OrderByMemberUsing(memberPath, "OrderBy");

    public static IOrderedQueryable<T> DynamicOrderByMemberDescending<T>(this IEnumerable<T> source, string memberPath) =>
        source.AsQueryable().OrderByMemberUsing(memberPath, "OrderByDescending");

    private static IOrderedQueryable<T> OrderByMemberUsing<T>(this IQueryable<T> source, string memberPath, string method)
    {
        var parameter = Expression.Parameter(typeof(T), $"DF_order_{typeof(T).Name}");
        var member = memberPath.Split('.').Aggregate((Expression)parameter, Expression.PropertyOrField);
        var keySelector = Expression.Lambda(member, parameter);
        var methodCall = Expression.Call(typeof(Queryable), method, [parameter.Type, member.Type], source.Expression, Expression.Quote(keySelector));
        return (IOrderedQueryable<T>)source.Provider.CreateQuery(methodCall);
    }

    private static Expression<Func<TEntity, bool>> GenerateConstantExpression<TEntity, TKeyValue>(string propertyName, TKeyValue keyValue)
    {
        var parameter = Expression.Parameter(typeof(TEntity), $"DF_ext_{typeof(TEntity).Name.ToUpper()}");
        var property = Expression.PropertyOrField(parameter, propertyName);
        var value = property.Type == typeof(TKeyValue) ? keyValue : property.Type.ParseValue(keyValue?.ToString());
        var equal = Expression.Equal(property, Expression.Constant(value, property.Type));
        return Expression.Lambda<Func<TEntity, bool>>(equal, parameter);
    }
}