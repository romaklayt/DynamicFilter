using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using romaklayt.DynamicFilter.Common.Interfaces;
using romaklayt.DynamicFilter.Common.Models;

namespace romaklayt.DynamicFilter.Parser;

public static class DynamicComplexParser
{
    public static ExpressionDynamicFilter<TSource, TTarget> BindExpressions<TSource, TTarget>(
        this IDynamicFilter complexModel)
    {
        if (complexModel == null) throw new ArgumentNullException(nameof(complexModel));
        var model = Activator.CreateInstance(typeof(ExpressionDynamicFilter<TSource, TTarget>));
        var itemType = typeof(ExpressionDynamicFilter<TSource, TTarget>).GenericTypeArguments[0];
        var parameter = Expression.Parameter(itemType, "x");
        ExtractFilters(model, complexModel, parameter, itemType);
        return model as ExpressionDynamicFilter<TSource, TTarget>;
    }

    public static ExpressionDynamicFilter<TSource, TTarget> BindExpressions<TSource, TTarget>(
        this IDynamicSorting complexModel)
    {
        if (complexModel == null) throw new ArgumentNullException(nameof(complexModel));
        var model = Activator.CreateInstance(typeof(ExpressionDynamicFilter<TSource, TTarget>));
        ExtractOrder(model, complexModel);
        return model as ExpressionDynamicFilter<TSource, TTarget>;
    }

    public static ExpressionDynamicFilter<TSource, TTarget> BindExpressions<TSource, TTarget>(
        this IDynamicSelect complexModel)
    {
        if (complexModel == null) throw new ArgumentNullException(nameof(complexModel));
        var model = Activator.CreateInstance(typeof(ExpressionDynamicFilter<TSource, TTarget>));
        ExtractSelect<TSource, TTarget>(model, complexModel);
        return model as ExpressionDynamicFilter<TSource, TTarget>;
    }

    public static ExpressionDynamicFilter<TSource, TTarget> BindExpressions<TSource, TTarget>(
        this IDynamicPaging complexModel)
    {
        if (complexModel == null) throw new ArgumentNullException(nameof(complexModel));
        var model = Activator.CreateInstance(typeof(ExpressionDynamicFilter<TSource, TTarget>));
        ExtractPagination(model, complexModel);
        return model as ExpressionDynamicFilter<TSource, TTarget>;
    }

    private static void ExtractPagination(object model, object bindingContext)
    {
        var page = bindingContext.GetType().GetProperty("Page")?.GetValue(bindingContext, null).ToString();
        var pageSize = bindingContext.GetType().GetProperty("PageSize")?.GetValue(bindingContext, null).ToString();

        if (!string.IsNullOrWhiteSpace(page))
            model.GetType().GetProperty("Page")?.SetValue(model, int.Parse(page));

        if (!string.IsNullOrWhiteSpace(pageSize))
            model.GetType().GetProperty("PageSize")?.SetValue(model, int.Parse(pageSize));
    }

    private static void ExtractSelect<TSource, TTarget>(object model, object bindingContext)
    {
        var select = bindingContext.GetType().GetProperty("Select")?.GetValue(bindingContext, null) as string;

        if (!string.IsNullOrWhiteSpace(select))
            model.GetType().GetProperty("Select")
                ?.SetValue(model, BuildSelector<TSource, TTarget>(select));
    }

    private static void ExtractOrder(object model, object bindingContext)
    {
        var order = bindingContext.GetType().GetProperty("Order")?.GetValue(bindingContext, null) as string;

        if (!string.IsNullOrWhiteSpace(order))
            model.GetType().GetProperty("Order")?.SetValue(model, order);
    }

    private static void ExtractFilters(object model, object bindingContext, ParameterExpression parameter,
        Type itemType)
    {
        var filter = bindingContext.GetType().GetProperty("Filter")?.GetValue(bindingContext, null) as string;
        if (string.IsNullOrWhiteSpace(filter)) return;
        var filterModel = new FilterArrayWrapper(filter, itemType, parameter);

        LambdaExpression finalExpression = null;
        if (filterModel.Expression != null) finalExpression = Expression.Lambda(filterModel.Expression, parameter);

        model.GetType().GetProperty("Filter")?.SetValue(model, finalExpression);
    }

    private static Expression<Func<TSource, TTarget>> BuildSelector<TSource, TTarget>(string members)
    {
        return BuildSelector<TSource, TTarget>(CheckRootMember(members, typeof(TTarget)));
    }

    private static Expression<Func<TSource, TTarget>> BuildSelector<TSource, TTarget>(List<string> members)
    {
        var parameter = Expression.Parameter(typeof(TSource), "e");
        var body = new List<MemberBinding>();
        var allMembers = members.OrderByDescending(s =>
            s.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Count()).ToList();
        var array = allMembers.Select(s => s.Trim().Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
            .ToList();
        foreach (var member in array)
        {
            BuildSelectorExpression(typeof(TTarget), parameter, member, out var list);
            body.AddRange(list);
        }

        return Expression.Lambda<Func<TSource, TTarget>>(
            Expression.MemberInit(Expression.New(typeof(TTarget)), body), parameter);
    }

    private static List<string> CheckRootMember(string filter, Type type)
    {
        var selectedMembers =
            filter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (!selectedMembers.Contains("root")) return selectedMembers;
        selectedMembers.Remove("root");
        selectedMembers.AddRange(type.GetProperties()
            .Where(info => IsSimple(info.PropertyType)).Select(info => FirstCharToLowerCase(info.Name)));
        return selectedMembers;
    }

    private static string FirstCharToLowerCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLower(str[0]) + str.Substring(1);
    }

    private static bool IsSimple(Type type)
    {
        return type != null && TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
    }

    private static Expression BuildSelectorExpression(Type targetType, Expression source,
        IReadOnlyList<string> membersPath, out List<MemberBinding> bindings, int depth = 0)
    {
        bindings = new List<MemberBinding>();
        var memberName = membersPath[depth];
        var target = Expression.Constant(null, targetType);
        var targetMember = Expression.PropertyOrField(target, memberName);
        var sourceMember = Expression.PropertyOrField(source, memberName);
        if (membersPath.Count == depth + 1)
        {
            bindings.Add(Expression.Bind(targetMember.Member, sourceMember));
        }
        else
        {
            Expression targetValue;
            if (IsEnumerableType(targetMember.Type, out var sourceElementType) &&
                IsEnumerableType(targetMember.Type, out var targetElementType))
            {
                var sourceElementParam = Expression.Parameter(sourceElementType, "e");
                targetValue = BuildSelectorExpression(targetElementType, sourceElementParam,
                    membersPath, out _, depth + 1);
                targetValue = Expression.Call(typeof(Enumerable), nameof(Enumerable.Select),
                    new[] { sourceElementType, targetElementType }, sourceMember,
                    Expression.Lambda(targetValue, sourceElementParam));
                targetValue = Expression.Condition(
                    Expression.Equal(sourceMember,
                        Expression.Constant(null, sourceMember.Type)),
                    Expression.Constant(null, sourceMember.Type),
                    CorrectEnumerableResult(targetValue, targetElementType, targetMember.Type),
                    sourceMember.Type);
            }
            else
            {
                targetValue = Expression.Condition(
                    Expression.Equal(sourceMember,
                        Expression.Constant(null, sourceMember.Type)),
                    Expression.Constant(null, sourceMember.Type), BuildSelectorExpression(targetMember.Type,
                        sourceMember,
                        membersPath, out _, depth + 1));
            }

            bindings.Add(Expression.Bind(targetMember.Member, targetValue));
        }


        return Expression.MemberInit(Expression.New(targetType), bindings);
    }


    private static bool IsEnumerableType(Type type, out Type elementType)
    {
        foreach (var intf in type.GetInterfaces())
            if (intf.IsGenericType && intf.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = intf.GetGenericArguments()[0];
                return true;
            }

        elementType = null;
        return false;
    }

    private static bool IsSameCollectionType(Type type, Type genericType, Type elementType)
    {
        var result = genericType.MakeGenericType(elementType).IsAssignableFrom(type);
        return result;
    }

    private static Expression CorrectEnumerableResult(Expression enumerable, Type elementType, Type memberType)
    {
        if (memberType == enumerable.Type)
            return enumerable;

        if (memberType.IsArray)
            return Expression.Call(typeof(Enumerable), nameof(Enumerable.ToArray), new[] { elementType },
                enumerable);

        if (IsSameCollectionType(memberType, typeof(List<>), elementType)
            || IsSameCollectionType(memberType, typeof(ICollection<>), elementType)
            || IsSameCollectionType(memberType, typeof(IReadOnlyList<>), elementType)
            || IsSameCollectionType(memberType, typeof(IReadOnlyCollection<>), elementType))
            return Expression.Call(typeof(Enumerable), nameof(Enumerable.ToList), new[] { elementType },
                enumerable);

        throw new NotImplementedException($"Not implemented transformation for type '{memberType.Name}'");
    }
}