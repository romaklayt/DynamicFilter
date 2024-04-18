using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace romaklayt.DynamicFilter.Common.Models;

internal class FilterElement
{
    public FilterElement(string element, Type type, Expression parameter)
    {
        var split = element.Split(FilterElementContainsOperators.GetOperators(), StringSplitOptions.None);
        Property = split.First().TrimEnd('!');
        Properties = GetNestedProp(Property, type);
        Value = Properties.LastOrDefault()?.PropertyType.ParseValue(split.Last().TrimStart('='));
        var op = typeof(FilterElementContainsOperators).GetFields().First(info => info.GetValue(null)?.ToString() == element.GetOperator(split.First(), split.Last())).Name;
        Operator = (FilterElementContainsOperatorEnum)Enum.Parse(typeof(FilterElementContainsOperatorEnum), op);
        IsNegative = split.First().EndsWith('!');
        UseAllFilterTypeForList = split.Last().StartsWith('=');
        Expression = GetExpression(parameter);
    }

    private string Property { get; }
    private List<PropertyInfo> Properties { get; }
    private object Value { get; }
    private FilterElementContainsOperatorEnum Operator { get; }
    private bool IsNegative { get; }
    private bool UseAllFilterTypeForList { get; }
    private bool ApplyToEnumerable { get; set; } = true;
    public Expression Expression { get; }

    private static MethodInfo GetEnumerableAllMethod => typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "All" && m.GetParameters().Length == 2);

    private static MethodInfo GetEnumerableAnyMethod => typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "Any" && m.GetParameters().Length == 2);

    private MethodInfo GetEnumerableMethod(Type genericType)
    {
        return UseAllFilterTypeForList
            ? GetEnumerableAllMethod?.MakeGenericMethod(genericType.GetGenericArguments().First())
            : GetEnumerableAnyMethod?.MakeGenericMethod(genericType.GetGenericArguments().First());
    }

    private Expression GetExpression(Expression parameter)
    {
        var constantExpression = Expression.Constant(Value);
        Expression returnExpression;
        ParameterExpression subParam = null;
        Expression baseExp = null;
        Type genericType = null;

        var propertyType = Properties.LastOrDefault()?.PropertyType;
        if (propertyType != null)
        {
            var nullableType = Nullable.GetUnderlyingType(propertyType);
            if (nullableType != null)
            {
                var type = typeof(Nullable<>).MakeGenericType(nullableType);
                constantExpression = Expression.Constant(Value, type);
            }
        }

        var enumerableLayers = new List<(MethodInfo, Expression, ParameterExpression)>();
        var body = parameter;
        var addSubParam = false;
        foreach (var member in Properties)
            if (member.PropertyType.IsGenericType && member.PropertyType.GetGenericTypeDefinition().GetInterfaces().Any(i => i.IsAssignableFrom(typeof(IEnumerable<>))) &&
                ApplyToEnumerable)
            {
                if (addSubParam)
                {
                    var genericTypeArgument = genericType.GetGenericArguments().First();
                    subParam = Expression.Parameter(genericTypeArgument, $"DF_col_sub_{genericTypeArgument.Name}_{member.Name}");
                    body = Expression.Property(subParam, member);

                    var method = GetEnumerableMethod(genericType);
                    if (method is not null) enumerableLayers.Add((method, baseExp, subParam));
                    baseExp = Expression.Property(subParam, member);
                }
                else
                {
                    baseExp = Expression.Property(body, member);
                    body = Expression.Property(body, member);
                }

                genericType = member.PropertyType;
                addSubParam = true;
            }
            else
            {
                if (addSubParam)
                {
                    var genericTypeArgument = member.PropertyType.GetGenericArguments().FirstOrDefault() ?? genericType.GetGenericArguments().First();
                    subParam = Expression.Parameter(genericTypeArgument, $"DF_sub_{genericTypeArgument.Name}_{member.Name}");
                    body = Expression.Property(subParam, member);
                    addSubParam = false;
                }
                else
                {
                    body = Expression.Property(body, member);
                }
            }

        returnExpression = GetOperatorExpression(body, constantExpression);

        if (genericType == null) return returnExpression;
        var listMethod = GetEnumerableMethod(genericType);

        if (listMethod is not null && returnExpression is not null && subParam is not null)
            returnExpression = Expression.Call(listMethod, baseExp, Expression.Lambda(returnExpression, subParam));

        enumerableLayers.Reverse();
        return enumerableLayers.Aggregate(returnExpression, (current, tuple) => Expression.Call(tuple.Item1, tuple.Item2, Expression.Lambda(current, tuple.Item3)));
    }

    private Expression GetOperatorExpression(Expression body, ConstantExpression constantExpression)
    {
        Expression returnExpression;
        switch (Operator)
        {
            default:
            case FilterElementContainsOperatorEnum.Equals:
                returnExpression = Expression.Equal(body, constantExpression);
                break;
            case FilterElementContainsOperatorEnum.ContainsCaseInsensitive:
                returnExpression = GetCustomCaseInsensitiveExpression(body, constantExpression, "Contains");
                break;
            case FilterElementContainsOperatorEnum.Contains:
                returnExpression = GetCustomExpression(body, constantExpression, "Contains");
                break;
            case FilterElementContainsOperatorEnum.GreaterThan:
                returnExpression = Expression.GreaterThan(body, constantExpression);
                break;
            case FilterElementContainsOperatorEnum.LessThan:
                returnExpression = Expression.LessThan(body, constantExpression);
                break;
            case FilterElementContainsOperatorEnum.GreaterOrEqual:
                returnExpression = Expression.GreaterThanOrEqual(body, constantExpression);
                break;
            case FilterElementContainsOperatorEnum.LessOrEqual:
                returnExpression = Expression.LessThanOrEqual(body, constantExpression);
                break;
            case FilterElementContainsOperatorEnum.StartsWith:
                returnExpression = GetCustomExpression(body, constantExpression, "StartsWith");
                break;
            case FilterElementContainsOperatorEnum.EndsWith:
                returnExpression = GetCustomExpression(body, constantExpression, "EndsWith");
                break;
            case FilterElementContainsOperatorEnum.StartsWithCaseInsensitive:
                returnExpression = GetCustomCaseInsensitiveExpression(body, constantExpression, "StartsWith");
                break;
            case FilterElementContainsOperatorEnum.EndsWithCaseInsensitive:
                returnExpression = GetCustomCaseInsensitiveExpression(body, constantExpression, "EndsWith");
                break;
            case FilterElementContainsOperatorEnum.EqualsCaseInsensitive:
                returnExpression = GetCustomCaseInsensitiveExpression(body, constantExpression, "Equals");
                break;
        }

        if (IsNegative) returnExpression = Expression.Not(returnExpression);
        return returnExpression;
    }

    private static Expression GetCustomCaseInsensitiveExpression(Expression body, Expression constantExpression, string methodName)
    {
        Expression returnExpression = null;

        var normalizeMethod = typeof(string).GetMethod("ToUpper", Type.EmptyTypes);

        if (normalizeMethod is not null)
        {
            var bodyNormalizeExpression = Expression.Call(body, normalizeMethod);
            var constantNormalizeExpression = Expression.Call(constantExpression, normalizeMethod);
            var method = typeof(string).GetMethod(methodName, [typeof(string)]);

            if (method is not null)
                returnExpression = Expression.Call(bodyNormalizeExpression, method, constantNormalizeExpression);
        }

        return returnExpression;
    }

    private static Expression GetCustomExpression(Expression body, Expression constantExpression, string methodName)
    {
        Expression returnExpression = null;
        var method = typeof(string).GetMethod(methodName, [typeof(string)]);

        if (method is not null) returnExpression = Expression.Call(body, method, constantExpression);
        return returnExpression;
    }

    #region [ Private Methods ]

    private List<PropertyInfo> GetNestedProp(string name, Type obj)
    {
        var infos = new List<PropertyInfo>();
        foreach (var part in name.Split('.'))
        {
            if (obj == null) return null;

            if (obj.IsGenericType && obj.GetGenericTypeDefinition().GetInterfaces().Any(i => i.IsAssignableFrom(typeof(IEnumerable<>))))
            {
                if (SupportedEnumerableProperties.GetOperators().Contains(part, StringComparer.OrdinalIgnoreCase))
                    ApplyToEnumerable = false;
                else
                    obj = obj.GetGenericArguments().FirstOrDefault();
            }

            if (obj != null)
            {
                var info = obj.GetProperty(part,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.GetField);
                if (info == null) return infos;

                obj = info.PropertyType;
                infos.Add(info);
            }
        }

        return infos;
    }

    #endregion
}