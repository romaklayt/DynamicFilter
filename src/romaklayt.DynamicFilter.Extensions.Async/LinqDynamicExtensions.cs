using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Extensions.Async;

public static class LinqDynamicExtensions
{
    public static async Task<int> CountAsync<TEntity>(this IAsyncQueryable<TEntity> source, IDynamicFilter dynamicFilter, CancellationToken cancellationToken = default)
        where TEntity : class =>
        await source.ApplyFilter(dynamicFilter, false).CountAsync(cancellationToken);

    public static async Task<int> CountAsync<TEntity>(this IAsyncEnumerable<TEntity> source, IDynamicFilter dynamicFilter, CancellationToken cancellationToken = default)
        where TEntity : class =>
        await source.AsAsyncQueryable().CountAsync(dynamicFilter, cancellationToken);

    public static async Task<TEntity> DynamicFirstOfDefaultAsync<TEntity, TKeyValue>(IAsyncQueryable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default)
        where TEntity : class =>
        await source.FirstOrDefaultAsync(GenerateConstantExpression<TEntity, TKeyValue>(propertyName, keyValue), cancellationToken);

    public static async Task<TEntity> DynamicFirstOfDefaultAsync<TEntity, TKeyValue>(IAsyncEnumerable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default)
        where TEntity : class =>
        await DynamicFirstOfDefaultAsync(source.AsAsyncQueryable(), propertyName, keyValue, cancellationToken);

    public static async Task<TEntity> DynamicFirstAsync<TEntity, TKeyValue>(IAsyncQueryable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default) where TEntity : class =>
        await source.FirstAsync(GenerateConstantExpression<TEntity, TKeyValue>(propertyName, keyValue), cancellationToken);

    public static async Task<TEntity> DynamicFirstAsync<TEntity, TKeyValue>(IAsyncEnumerable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default) where TEntity : class =>
        await DynamicFirstOfDefaultAsync(source.AsAsyncQueryable(), propertyName, keyValue, cancellationToken);

    public static async Task<TEntity> DynamicLastOfDefaultAsync<TEntity, TKeyValue>(IAsyncQueryable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default)
        where TEntity : class =>
        await source.LastOrDefaultAsync(GenerateConstantExpression<TEntity, TKeyValue>(propertyName, keyValue), cancellationToken);

    public static async Task<TEntity> DynamicLastOfDefaultAsync<TEntity, TKeyValue>(IAsyncEnumerable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default)
        where TEntity : class =>
        await DynamicLastOfDefaultAsync(source.AsAsyncQueryable(), propertyName, keyValue, cancellationToken);

    public static async Task<TEntity> DynamicLastAsync<TEntity, TKeyValue>(IAsyncQueryable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default) where TEntity : class =>
        await source.LastAsync(GenerateConstantExpression<TEntity, TKeyValue>(propertyName, keyValue), cancellationToken);

    public static async Task<TEntity> DynamicLastAsync<TEntity, TKeyValue>(IAsyncEnumerable<TEntity> source, string propertyName, TKeyValue keyValue,
        CancellationToken cancellationToken = default) where TEntity : class =>
        await DynamicLastOfDefaultAsync(source.AsAsyncQueryable(), propertyName, keyValue, cancellationToken);

    public static IAsyncQueryable<TEntity> DynamicOrderBy<TEntity>(this IAsyncQueryable<TEntity> source, params Tuple<string, bool>[] order)
    {
        IOrderedAsyncQueryable<TEntity> orderExpression = null;
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

    public static IOrderedAsyncQueryable<T> DynamicOrderByMember<T>(this IAsyncQueryable<T> source, string memberPath) => source.OrderByMemberUsing(memberPath, "OrderBy");

    public static IOrderedAsyncQueryable<T> DynamicOrderByMemberDescending<T>(this IAsyncQueryable<T> source, string memberPath) =>
        source.OrderByMemberUsing(memberPath, "OrderByDescending");

    public static IOrderedAsyncQueryable<T> DynamicThenByMember<T>(this IOrderedAsyncQueryable<T> source, string memberPath) => source.OrderByMemberUsing(memberPath, "ThenBy");

    public static IOrderedAsyncQueryable<T> DynamicThenByMemberDescending<T>(this IOrderedAsyncQueryable<T> source, string memberPath) =>
        source.OrderByMemberUsing(memberPath, "ThenByDescending");

    public static IOrderedAsyncQueryable<T> DynamicOrderByMember<T>(this IOrderedAsyncEnumerable<T> source, string memberPath) =>
        source.AsAsyncQueryable().OrderByMemberUsing(memberPath, "OrderBy");

    public static IOrderedAsyncQueryable<T> DynamicOrderByMemberDescending<T>(this IOrderedAsyncEnumerable<T> source, string memberPath) =>
        source.AsAsyncQueryable().OrderByMemberUsing(memberPath, "OrderByDescending");

    private static IOrderedAsyncQueryable<T> OrderByMemberUsing<T>(this IAsyncQueryable<T> source, string memberPath, string method)
    {
        var parameter = Expression.Parameter(typeof(T), $"DF_order_{typeof(T).Name}");
        var member = memberPath.Split('.').Aggregate((Expression)parameter, Expression.PropertyOrField);
        var keySelector = Expression.Lambda(member, parameter);
        var methodCall = Expression.Call(typeof(AsyncQueryable), method, [parameter.Type, member.Type], source.Expression, Expression.Quote(keySelector));
        return (IOrderedAsyncQueryable<T>)source.Provider.CreateQuery<T>(methodCall);
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