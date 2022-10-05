using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Parser;

public static class DynamicComplexParser
{
    public static ExpressionDynamicFilter<TSource, TTarget> BindFilterExpressions<TSource, TTarget>(
        this BaseDynamicComplexModel complexModel)
    {
        if (complexModel == null) throw new ArgumentNullException(nameof(complexModel));
        var model = Activator.CreateInstance(typeof(ExpressionDynamicFilter<TSource, TTarget>));
        var itemType = typeof(ExpressionDynamicFilter<TSource, TTarget>).GenericTypeArguments[0];
        var parameter = Expression.Parameter(itemType, "x");
        ExtractFilters(model, complexModel, parameter, itemType);
        ExtractOrder(model, complexModel, parameter);
        ExtractPagination(model, complexModel);
        ExtractSelect<TSource, TTarget>(model, complexModel);
        return model as ExpressionDynamicFilter<TSource, TTarget>;
    }

    private static void ExtractPagination(object model, object bindingContext)
    {
        var page = bindingContext.GetType().GetProperty("Page")?.GetValue(bindingContext, null).ToString();
        var pageSize = bindingContext.GetType().GetProperty("PageSize")?.GetValue(bindingContext, null).ToString();
        if (!string.IsNullOrWhiteSpace(page)) model.GetType().GetProperty("Page")?.SetValue(model, int.Parse(page));
        if (!string.IsNullOrWhiteSpace(pageSize))
            model.GetType().GetProperty("PageSize")?.SetValue(model, int.Parse(pageSize));
    }

    internal static void ExtractSelect<TSource, TTarget>(object model, object bindingContext)
    {
        var select = bindingContext.GetType().GetProperty("Select")?.GetValue(bindingContext, null) as string;
        if (!string.IsNullOrWhiteSpace(select))
            model.GetType().GetProperty("Select")?.SetValue(model, BuildSelector<TSource, TTarget>(select));
    }

    private static void ExtractOrder(object model, object bindingContext, ParameterExpression parameter)
    {
        var order = bindingContext.GetType().GetProperty("Order")?.GetValue(bindingContext, null) as string;
        if (!string.IsNullOrWhiteSpace(order)) model.GetType().GetProperty("Order")?.SetValue(model, order);
    }

    private static string RemoveSubstring(string sourceString, string removeString)
    {
        var index = sourceString.IndexOf(removeString, StringComparison.InvariantCulture);
        return index < 0 ? sourceString : sourceString.Remove(index, removeString.Length);
    }

    internal static void ExtractFilters(object model, object bindingContext, ParameterExpression parameter,
        Type itemType)
    {
        var filter = bindingContext.GetType().GetProperty("Filter")?.GetValue(bindingContext, null) as string;
        var operators = new[] { "=", "%", "%%", ">", ">=", "<", "<=", "!=" };
        if (string.IsNullOrWhiteSpace(filter)) return;
        var temp = filter.Split(',').ToList();
        var filterAndValues = new List<string>();
        foreach (var f in temp)
        {
            var split = f.Split(operators, StringSplitOptions.RemoveEmptyEntries);
            var property = split.First();
            var op = RemoveSubstring(RemoveSubstring(f, split.First()), split.Last());
            if (split.Last().StartsWith("[") && split.Last().EndsWith("]"))
            {
                if (split.Last().Contains("~"))
                {
                    var values = split.Last().Trim('[', ']').Split(new[] { "~" }, StringSplitOptions.None);
                    filterAndValues.AddRange(values.Select(value => $"{property}{op}{value}"));
                    continue;
                }

                if (split.Last().Contains("|"))
                {
                    var values = split.Last().Trim('[', ']').Split(new[] { "|" }, StringSplitOptions.None);
                    filterAndValues.Add(values
                        .Aggregate(string.Empty, (current, value) => current + $"|{property}{op}{value}")
                        .TrimStart('|'));
                    continue;
                }
            }

            filterAndValues.Add(f);
        }

        LambdaExpression finalExpression = null;
        Expression currentExpression = null;
        for (var i = 0; i < filterAndValues.Count(); i++)
            if (filterAndValues[i].Contains('|'))
            {
                var options = filterAndValues[i].Split('|');
                Expression splitExpression = null;
                for (var j = 0; j < options.Count(); j++)
                {
                    var split = options[j].Split(operators, StringSplitOptions.None);
                    var expression = GetExpression(parameter, itemType,
                        $"{split.First()}{RemoveSubstring(RemoveSubstring(options[j], split.First()), split.Last())}{split.Last()}");
                    if (j == 0)
                    {
                        splitExpression = expression;
                    }
                    else
                    {
                        if (splitExpression != null) splitExpression = Expression.Or(splitExpression, expression);
                    }
                }

                currentExpression = currentExpression == null ? splitExpression :
                    splitExpression == null ? currentExpression : Expression.And(currentExpression, splitExpression);
            }
            else
            {
                var expression = GetExpression(parameter, itemType, filterAndValues[i]);
                currentExpression = currentExpression == null
                    ? expression
                    : Expression.And(currentExpression, expression);
            }

        if (currentExpression != null) finalExpression = Expression.Lambda(currentExpression, parameter);
        model.GetType().GetProperty("Filter")?.SetValue(model, finalExpression);
    }

    private static Expression GetExpression(ParameterExpression parameter, Type itemType, string filterAndValue)
    {
        var expressionType = new ExpressionParser(filterAndValue, itemType);
        var expression = expressionType.GetExpression(parameter);
        return expression;
    }

    private static Expression<Func<TSource, TTarget>> BuildSelector<TSource, TTarget>(string members)
    {
        members = SelectedRender.CheckRootMember(members, typeof(TSource));
        SelectedRender.UpdateMembersPath(members, typeof(TSource), out var items, true);
        return BuildSelector<TSource, TTarget>(items);
    }

    private static Expression<Func<TSource, TTarget>> BuildSelector<TSource, TTarget>(List<string> members)
    {
        var parameter = Expression.Parameter(typeof(TSource), "e");
        var body = new List<MemberBinding>();
        var allMembers = members.OrderByDescending(s =>
            s.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Count()).ToList();
        var array = allMembers.Select(s => s.Trim().Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
            .OrderByDescending(strings => strings.Length).ToList();
        var groups = array.GroupBy(strings => strings[0]);
        BuildSelectorExpression(typeof(TTarget), parameter, groups, out var list);
        body.AddRange(list);
        return Expression.Lambda<Func<TSource, TTarget>>(Expression.MemberInit(Expression.New(typeof(TTarget)), body),
            parameter);
    }

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
                    bindings.Add(Expression.Bind(member.Member, sourceMember));
                }
                else
                {
                    var rootMembersForType = GetTypeSimpleProperties(
                        IsEnumerableType(targetMember.Type, out var sourceType) ? sourceType : member.Type);
                    destinationMembers = destinationMembers
                        .Union(rootMembersForType.Select(s => rootMember.Union(new[] { s }).ToArray())).ToList();
                }
            }

            destinationMembers = destinationMembers.Distinct(new CustomEnumerableComparer<string>())
                .Select(enumerable => enumerable.ToArray()).ToList();
            if (!destinationMembers.Any()) continue;
            Expression targetValue;
            if (IsEnumerableType(targetMember.Type, out var sourceElementType) &&
                IsEnumerableType(targetMember.Type, out var targetElementType))
            {
                var sourceElementParam = Expression.Parameter(sourceElementType, "e");
                targetValue = BuildSelectorExpression(targetElementType, sourceElementParam,
                    destinationMembers.GroupBy(strings => strings[depth + 1]), out _, depth + 1);
                targetValue = Expression.Call(typeof(Enumerable), nameof(Enumerable.Select),
                    new[] { sourceElementType, targetElementType }, sourceMember,
                    Expression.Lambda(targetValue, sourceElementParam));
                targetValue = Expression.Condition(
                    Expression.Equal(sourceMember, Expression.Constant(null, sourceMember.Type)),
                    Expression.Constant(null, sourceMember.Type),
                    CorrectEnumerableResult(targetValue, targetElementType, targetMember.Type), sourceMember.Type);
            }
            else
            {
                targetValue = Expression.Condition(
                    Expression.Equal(sourceMember, Expression.Constant(null, sourceMember.Type)),
                    Expression.Constant(null, sourceMember.Type),
                    BuildSelectorExpression(targetMember.Type, sourceMember,
                        destinationMembers.GroupBy(strings => strings[depth + 1]), out _, depth + 1));
            }

            bindings.Add(Expression.Bind(targetMember.Member, targetValue));
        }

        return Expression.MemberInit(Expression.New(targetType), bindings);
    }

    private static IEnumerable<string> GetTypeSimpleProperties(Type type) => type.GetProperties()
        .Where(info => IsSimple(info.PropertyType)).Select(info => FirstCharToLowerCase(info.Name));

    private static bool IsSimple(Type type) =>
        type != null && TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));

    private static string FirstCharToLowerCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0])) return str;
        return char.ToLower(str[0]) + str.Substring(1);
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
        if (memberType == enumerable.Type) return enumerable;
        if (memberType.IsArray)
            return Expression.Call(typeof(Enumerable), nameof(Enumerable.ToArray), new[] { elementType }, enumerable);
        if (IsSameCollectionType(memberType, typeof(List<>), elementType) ||
            IsSameCollectionType(memberType, typeof(ICollection<>), elementType) ||
            IsSameCollectionType(memberType, typeof(IReadOnlyList<>), elementType) ||
            IsSameCollectionType(memberType, typeof(IReadOnlyCollection<>), elementType))
            return Expression.Call(typeof(Enumerable), nameof(Enumerable.ToList), new[] { elementType }, enumerable);
        throw new NotImplementedException($"Not implemented transformation for type '{memberType.Name}'");
    }
}