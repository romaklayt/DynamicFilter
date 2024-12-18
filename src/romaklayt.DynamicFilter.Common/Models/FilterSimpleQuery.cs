using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace romaklayt.DynamicFilter.Common.Models;

internal class ParsedExpression
{
    public ParsedExpression(ParameterExpression subParam, Expression baseExp, Type genericType, List<(MethodInfo, Expression, ParameterExpression)> enumerableLayers,
        Expression body)
    {
        SubParam = subParam;
        BaseExp = baseExp;
        GenericType = genericType;
        EnumerableLayers = enumerableLayers;
        Body = body;
    }

    public ParameterExpression SubParam { get; set; }
    public Expression BaseExp { get; set; }
    public Type GenericType { get; set; }
    public List<(MethodInfo, Expression, ParameterExpression)> EnumerableLayers { get; set; }
    public Expression Body { get; set; }
}

internal record FilterElement(string? Path, string Value, ElementOperatorEnum Operator, bool IsNegative);

internal partial class FilterSimpleQuery
{
    private static readonly MethodInfo GetEnumerableAllMethod =
        typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "All" && m.GetParameters().Length == 2);

    private static readonly MethodInfo GetEnumerableAnyMethod =
        typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "Any" && m.GetParameters().Length == 2);

    private readonly bool _isAllEnumerable;

    public FilterSimpleQuery(string element, Type type, Expression parameter)
    {
        List<PropertyInfo> properties;
        var enumerableMatch = ParseEnumerableRegex().Match(element);
        if (!enumerableMatch.Success)
        {
            var parse = Parse(element);
            if (!parse.HasValue) throw new ArgumentException("Invalid filter element");
            properties = GetNestedProp(parse.Value.Path.TrimEnd('!'), type);
            Dictionary.Add(new FilterElement(null, parse.Value.Value, parse.Value.Op, parse.Value.Path.EndsWith('!')));
        }
        else
        {
            var path = enumerableMatch.Groups["path"].Value;
            var opStr = enumerableMatch.Groups["operator"].Value;
            var conditions = enumerableMatch.Groups["conditions"].Value;
            EnumerableOperators.Get.TryGetValue(opStr, out var op);
            _isAllEnumerable = op == EnumerableOperatorEnum.All;
            properties = GetNestedProp(path, type);
            var split = conditions.Split(FilterEnumerableLogicOperators.Get.Keys.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            EnumerableOperator = split.Aggregate(conditions, (current, substring) => current.Replace(substring, string.Empty)).ToList();
            foreach (var item in split)
            {
                var parse = Parse(item);
                if (!parse.HasValue) throw new ArgumentException("Invalid filter element");
                Dictionary.Add(new FilterElement(parse.Value.Path.TrimEnd('!'), parse.Value.Value, parse.Value.Op, parse.Value.Path.EndsWith('!')));
            }
        }

        Expression = GetExpression(parameter, properties);
    }

    private List<FilterElement> Dictionary { get; } = [];
    public Expression Expression { get; }
    private List<char> EnumerableOperator { get; }

    private static (string Path, ElementOperatorEnum Op, string Value)? Parse(string str)
    {
        var match = ParseElementRegex().Match(str);
        if (match.Success)
        {
            var path = match.Groups["path"].Value;
            var opStr = match.Groups["operator"].Value;
            var value = match.Groups["value"].Value;
            ElementOperators.Get.TryGetValue(opStr, out var op);
            return (path, op, value);
        }

        return null;
    }

    [GeneratedRegex(@"^(?<path>[\w\.]+)(?<operator>==\*|_-=\*|_=\*|@=\*|_-=|_=|<=|>=|<|>|@=|==)(?<value>.+)$")]
    private static partial Regex ParseElementRegex();

    [GeneratedRegex(@"^(?<path>[\w\.]+):(?<operator>\w+)\[(?<conditions>.+)\]$")]
    private static partial Regex ParseEnumerableRegex();

    private MethodInfo GetEnumerableMethod(Type genericType)
    {
        var method = _isAllEnumerable ? GetEnumerableAllMethod : GetEnumerableAnyMethod;
        return method!.MakeGenericMethod(genericType.GetGenericArguments().First());
    }

    private Expression GetExpression(Expression parameter, List<PropertyInfo> properties)
    {
        var parsedExpression = PerformProps(parameter, properties);

        Expression returnExpr = null;
        var index = 0;
        foreach (var tuple in Dictionary)
        {
            var props = GetNestedProp(tuple.Path, parsedExpression.GenericType);
            var constantExpression = GetConstantExpression(properties, props, tuple);
            Expression operatorExp;
            if (props.Count == 0)
            {
                operatorExp = GetOperatorExpression(parsedExpression.Body, constantExpression, tuple.Operator, tuple.IsNegative);
            }
            else
            {
                var genericTypeArgument = parsedExpression.GenericType?.GetGenericArguments().First();
                if (genericTypeArgument is not null)
                    parsedExpression.SubParam = parsedExpression.SubParam?.Type == genericTypeArgument
                        ? parsedExpression.SubParam
                        : Expression.Parameter(genericTypeArgument, $"DF_sub_{genericTypeArgument.Name}");
                var parsedEnumerableExpr = PerformProps(parsedExpression.SubParam, props);
                operatorExp = GetOperatorExpression(parsedEnumerableExpr.Body, constantExpression, tuple.Operator, tuple.IsNegative);
                if (parsedEnumerableExpr.GenericType is not null)
                {
                    var listMethod2 = GetEnumerableMethod(parsedEnumerableExpr.GenericType);
                    if (listMethod2 is not null && operatorExp is not null && parsedEnumerableExpr.SubParam is not null)
                        operatorExp = Expression.Call(listMethod2, parsedEnumerableExpr.BaseExp, Expression.Lambda(operatorExp, parsedEnumerableExpr.SubParam));
                    parsedExpression.EnumerableLayers.Reverse();
                    operatorExp = parsedExpression.EnumerableLayers.Aggregate(operatorExp, (current, t) => Expression.Call(t.Item1, t.Item2, Expression.Lambda(current, t.Item3)));
                }
            }

            returnExpr =
                returnExpr is null
                    ? operatorExp
                    : FilterEnumerableLogicOperators.Get.TryGetValue(EnumerableOperator[index++], out var e)
                      && e == FilterArrayLogicOperatorEnum.And
                        ? Expression.And(returnExpr, operatorExp)
                        : Expression.Or(returnExpr, operatorExp);
        }

        if (parsedExpression.GenericType == null) return returnExpr;
        var listMethod = GetEnumerableMethod(parsedExpression.GenericType);

        if (listMethod is not null && returnExpr is not null && parsedExpression.SubParam is not null)
            returnExpr = Expression.Call(listMethod, parsedExpression.BaseExp, Expression.Lambda(returnExpr, parsedExpression.SubParam));

        parsedExpression.EnumerableLayers.Reverse();
        return parsedExpression.EnumerableLayers.Aggregate(returnExpr,
            (current, tuple) => Expression.Call(tuple.Item1, tuple.Item2, Expression.Lambda(current, tuple.Item3)));
    }

    private static ConstantExpression GetConstantExpression(List<PropertyInfo> properties, List<PropertyInfo> props, FilterElement tuple)
    {
        var propertyType = props.LastOrDefault()?.PropertyType ?? properties.LastOrDefault()?.PropertyType;
        var value = propertyType.ParseValue(tuple.Value);
        var constantExpression = Expression.Constant(value);
        var nullableType = Nullable.GetUnderlyingType(propertyType);
        if (nullableType != null)
        {
            var type = typeof(Nullable<>).MakeGenericType(nullableType);
            constantExpression = Expression.Constant(value, type);
        }

        return constantExpression;
    }

    private ParsedExpression PerformProps(Expression parameter, List<PropertyInfo> properties)
    {
        ParameterExpression subParam = null;
        Expression baseExp = null;
        Type genericType = null;

        var enumerableLayers = new List<(MethodInfo, Expression, ParameterExpression)>();
        var body = parameter;
        var addSubParam = false;
        var applyEnumerable = !SupportedEnumerableProperties.Get.Intersect(properties.Select(x => x.Name), StringComparer.OrdinalIgnoreCase).Any();
        foreach (var member in properties)
            if (applyEnumerable && member.PropertyType.IsGenericType &&
                member.PropertyType.GetGenericTypeDefinition().GetInterfaces().Any(i => i.IsAssignableFrom(typeof(IEnumerable<>))))
            {
                if (addSubParam)
                {
                    var genericTypeArgument = genericType.GetGenericArguments().First();
                    subParam = Expression.Parameter(genericTypeArgument, $"DF_col_sub_{genericTypeArgument.Name}");
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
                    subParam = Expression.Parameter(genericTypeArgument, $"DF_sub_{genericTypeArgument.Name}");
                    body = Expression.Property(subParam, member);
                    addSubParam = false;
                }
                else
                {
                    body = Expression.Property(body, member);
                }
            }

        return new ParsedExpression(subParam, baseExp, genericType, enumerableLayers, body);
    }

    private static Expression GetOperatorExpression(Expression body, ConstantExpression constant, ElementOperatorEnum op, bool isNegative)
    {
        Expression returnExpression;
        switch (op)
        {
            default:
            case ElementOperatorEnum.Equals:
                returnExpression = Expression.Equal(body, constant);
                break;
            case ElementOperatorEnum.ContainsCaseInsensitive:
                returnExpression = GetCustomCaseInsensitiveExpression(body, constant, "Contains");
                break;
            case ElementOperatorEnum.Contains:
                returnExpression = GetCustomExpression(body, constant, "Contains");
                break;
            case ElementOperatorEnum.GreaterThan:
                returnExpression = Expression.GreaterThan(body, constant);
                break;
            case ElementOperatorEnum.LessThan:
                returnExpression = Expression.LessThan(body, constant);
                break;
            case ElementOperatorEnum.GreaterOrEqual:
                returnExpression = Expression.GreaterThanOrEqual(body, constant);
                break;
            case ElementOperatorEnum.LessOrEqual:
                returnExpression = Expression.LessThanOrEqual(body, constant);
                break;
            case ElementOperatorEnum.StartsWith:
                returnExpression = GetCustomExpression(body, constant, "StartsWith");
                break;
            case ElementOperatorEnum.EndsWith:
                returnExpression = GetCustomExpression(body, constant, "EndsWith");
                break;
            case ElementOperatorEnum.StartsWithCaseInsensitive:
                returnExpression = GetCustomCaseInsensitiveExpression(body, constant, "StartsWith");
                break;
            case ElementOperatorEnum.EndsWithCaseInsensitive:
                returnExpression = GetCustomCaseInsensitiveExpression(body, constant, "EndsWith");
                break;
            case ElementOperatorEnum.EqualsCaseInsensitive:
                returnExpression = GetCustomCaseInsensitiveExpression(body, constant, "Equals");
                break;
        }

        if (isNegative) returnExpression = Expression.Not(returnExpression);
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

    private static List<PropertyInfo> GetNestedProp(string path, Type obj)
    {
        if (string.IsNullOrWhiteSpace(path)) return [];

        var infos = new List<PropertyInfo>();
        foreach (var part in path.Split('.'))
        {
            if (obj == null || string.IsNullOrWhiteSpace(part)) return [];

            if (obj.IsGenericType && obj.GetGenericTypeDefinition().GetInterfaces().Any(i => i.IsAssignableFrom(typeof(IEnumerable<>))))
                if (!SupportedEnumerableProperties.Get.Contains(part, StringComparer.OrdinalIgnoreCase))
                    obj = obj.GetGenericArguments().FirstOrDefault();

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
}