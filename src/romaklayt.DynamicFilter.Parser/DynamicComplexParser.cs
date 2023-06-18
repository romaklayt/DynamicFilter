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
        var model = new ExpressionDynamicFilter<TSource, TTarget>();
        var parameter = Expression.Parameter(typeof(TSource), $"DF_{typeof(TSource).Name}");
        ExtractFilters(model, complexModel, parameter, typeof(TSource));
        ExtractOrder(model, complexModel);
        return model;
    }

    public static ExpressionDynamicFilter<TSource, TTarget> BindExpressions<TSource, TTarget>(
        this IDynamicSelect complexModel)
    {
        if (complexModel == null) throw new ArgumentNullException(nameof(complexModel));
        var model = new ExpressionDynamicFilter<TSource, TTarget>();
        ExtractSelect(model, complexModel);
        return model;
    }

    public static ExpressionDynamicFilter<TSource, TTarget> BindExpressions<TSource, TTarget>(
        this IDynamicPaging complexModel)
    {
        if (complexModel == null) throw new ArgumentNullException(nameof(complexModel));
        var model = new ExpressionDynamicFilter<TSource, TTarget>();
        ExtractPagination(model, complexModel);
        return model;
    }

    private static void ExtractPagination<TSource, TTarget>(ExpressionDynamicFilter<TSource, TTarget> model,
        IDynamicPaging bindingContext)
    {
        model.Page = bindingContext.Page;
        model.PageSize = bindingContext.PageSize;
    }

    private static void ExtractSelect<TSource, TTarget>(ExpressionDynamicFilter<TSource, TTarget> model,
        IDynamicSelect bindingContext)
    {
        var select = string.IsNullOrWhiteSpace(bindingContext.Select) ? new List<string>() : bindingContext.Select
            .Split(new[] { ',' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        var rootProperties = GetTypeSimpleProperties(typeof(TTarget));
        if (!select.Intersect(rootProperties).Any()) select.AddRange(rootProperties);
        model.Select = BuildSelector<TSource, TTarget>(select.Distinct().ToList());
    }

    private static void ExtractOrder<TSource, TTarget>(ExpressionDynamicFilter<TSource, TTarget> model,
        IDynamicFilter bindingContext) => model.Order = bindingContext.Order;

    private static void ExtractFilters<TSource, TTarget>(ExpressionDynamicFilter<TSource, TTarget> model,
        IDynamicFilter bindingContext, ParameterExpression parameter,
        Type itemType)
    {
        var filter = bindingContext.Filter;
        if (string.IsNullOrWhiteSpace(filter)) return;
        var filterModel = new FilterArrayWrapper(filter, itemType, parameter);

        Expression finalExpression = null;
        if (filterModel.Expression != null) finalExpression = Expression.Lambda(filterModel.Expression, parameter);

        model.Filter = finalExpression as Expression<Func<TSource, bool>>;
    }

    private static Expression<Func<TSource, TTarget>> BuildSelector<TSource, TTarget>(List<string> members)
    {
        var parameter = Expression.Parameter(typeof(TSource), $"DF_selector_{typeof(TSource).Name}");
        var body = new List<MemberBinding>();
        var allMembers = members
            .Select(s => s.Split(new[] { '.' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .OrderByDescending(strings => strings.Length).GroupBy(strings => strings[0]);
        BuildSelectorExpression(typeof(TTarget), parameter, allMembers, out var list);
        body.AddRange(list);


        return Expression.Lambda<Func<TSource, TTarget>>(
            Expression.MemberInit(Expression.New(typeof(TTarget)), body), parameter);
    }

    private static string FirstCharToLowerCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLower(str[0]) + str[1..];
    }

    private static bool IsSimple(Type type) =>
        type != null && TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));

    private static Expression BuildSelectorExpression(Type targetType, Expression source,
        IEnumerable<IGrouping<string, string[]>> groups, out List<MemberBinding> bindings, int depth = 0)
    {
        bindings = new List<MemberBinding>();
        foreach (var membersPaths in groups)
        {
            var target = Expression.Constant(null, targetType);
            var targetMember = Expression.PropertyOrField(target, membersPaths.Key);
            var sourceMember = Expression.PropertyOrField(source, membersPaths.Key);

            var rootMembers = membersPaths.Where(strings => strings.Length == depth + 1);
            var destinationMembers = membersPaths.Where(strings => strings.Length > depth + 1).ToList();
            foreach (var rootMember in rootMembers.Distinct())
            {
                var member = Expression.PropertyOrField(target, rootMember.Last());
                if (IsSimple(member.Type))
                {
                    bindings.Add(
                        Expression.Bind(member.Member, sourceMember));
                }
                else
                {
                    var rootMembersForType = GetTypeSimpleProperties(
                        IsEnumerableType(targetMember.Type, out var sourceType)
                            ? sourceType
                            : member.Type);

                    destinationMembers = destinationMembers
                        .Union(rootMembersForType.Select(s => rootMember.Union(new[] { s }).ToArray()))
                        .ToList();
                }
            }

            destinationMembers = destinationMembers.Distinct(new CustomEnumerableComparer<string>())
                .Select(enumerable => enumerable.ToArray()).ToList();
            if (!destinationMembers.Any()) continue;
            Expression targetValue;
            if (IsEnumerableType(targetMember.Type, out var sourceElementType) &&
                IsEnumerableType(targetMember.Type, out var targetElementType))
            {
                var sourceElementParam = Expression.Parameter(sourceElementType,
                    $"DF_list_selector_{sourceElementType?.GetType().Name}");
                targetValue = BuildSelectorExpression(targetElementType, sourceElementParam,
                    destinationMembers.GroupBy(strings => strings[depth + 1]), out _, depth + 1);
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
                        sourceMember, destinationMembers.GroupBy(strings => strings[depth + 1]), out _, depth + 1));
            }

            bindings.Add(Expression.Bind(targetMember.Member, targetValue));
        }

        return Expression.MemberInit(Expression.New(targetType), bindings);
    }

    private static List<string> GetTypeSimpleProperties(Type type) =>
        type.GetProperties()
            .Where(info => IsSimple(info.PropertyType) && (info.GetSetMethod(true)?.IsPublic ?? false))
            .Select(info => FirstCharToLowerCase(info.Name)).ToList();


    private static bool IsEnumerableType(Type type, out Type elementType)
    {
        elementType = type.GetGenericArguments().FirstOrDefault();
        return elementType != null;
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