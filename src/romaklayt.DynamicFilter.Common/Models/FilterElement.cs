using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace romaklayt.DynamicFilter.Common.Models;

internal class FilterElement
{
    private const string DefaultValue = @"\default";

    public FilterElement(string element, Type type, Expression parameter)
    {
        var split = element.Split(FilterElementContainsOperators.GetOperators(), StringSplitOptions.None);
        Property = split.First().TrimEnd('!');
        Properties = GetNestedProp(Property, type);
        Value = ParseValue(split.Last(), Properties.LastOrDefault()?.PropertyType);
        var op = typeof(FilterElementContainsOperators).GetFields().First(info =>
            info.GetValue(null).ToString() == element.GetOperator(split.First(), split.Last())).Name;
        Operator = (FilterElementContainsOperatorEnum)Enum.Parse(typeof(FilterElementContainsOperatorEnum), op);
        IsNegative = split.First().EndsWith("!");
        Expression = GetExpression(parameter);
    }

    private string Property { get; }
    private List<PropertyInfo> Properties { get; }
    private object Value { get; }
    private FilterElementContainsOperatorEnum Operator { get; }
    private bool IsNegative { get; }
    private bool ApplyToEnumerable { get; set; } = true;
    public Expression Expression { get; }

    private object GetDefaultValue(Type type)
    {
        if (type == null) throw new ArgumentNullException("type");
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private Expression GetExpression(Expression parameter)
    {
        var constantExpression = Expression.Constant(Value);
        Expression returnExpression;
        ParameterExpression subParam = null;
        Expression baseExp = null;
        Type genericType = null;

        var propertyType = Properties.LastOrDefault()?.PropertyType;
        if (propertyType != null && Nullable.GetUnderlyingType(propertyType) != null)
        {
            var type = typeof(Nullable<>).MakeGenericType(
                Nullable.GetUnderlyingType(propertyType));
            constantExpression = Expression.Constant(Value, type);
        }

        var body = parameter;
        foreach (var member in Properties)
            if (member.PropertyType.IsGenericType && member.PropertyType.GetGenericTypeDefinition().GetInterfaces()
                    .Any(i => i.IsAssignableFrom(typeof(IEnumerable<>))) && ApplyToEnumerable)
            {
                genericType = member.PropertyType;
                baseExp = Expression.Property(body, member);
                body = Expression.Property(body, member);
            }
            else
            {
                var genericArgumentFullName = genericType?.GetGenericArguments().FirstOrDefault()?.FullName;
                var genericArgumentBaseTypeFullName =
                    genericType?.GetGenericArguments().FirstOrDefault()?.BaseType?.FullName;
                var memberFullName = member.DeclaringType?.FullName;
                if (genericArgumentFullName == memberFullName || genericArgumentBaseTypeFullName == memberFullName)
                {
                    subParam = Expression.Parameter(genericType?.GetGenericArguments().FirstOrDefault() ??
                                                    throw new InvalidOperationException("Generic type not found"),
                        "y");
                    body = Expression.Property(subParam, member);
                }
                else
                {
                    body = Expression.Property(body, member);
                }
            }

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

        if (genericType == null) return returnExpression;
        var anyMethod = typeof(Enumerable)
            .GetMethods()
            .FirstOrDefault(m => m.Name == "Any" && m.GetParameters().Count() == 2)
            ?.MakeGenericMethod(genericType.GetGenericArguments().FirstOrDefault());


        if (constantExpression.Value != null && anyMethod is not null && returnExpression is not null)
            returnExpression =
                Expression.Call(anyMethod, baseExp, Expression.Lambda(returnExpression, subParam));

        return returnExpression;
    }

    private static Expression GetCustomCaseInsensitiveExpression(Expression body, ConstantExpression constantExpression,
        string methodName)
    {
        Expression returnExpression = null;

        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

        if (toLowerMethod is not null)
        {
            var expression1 = Expression.Call(body, toLowerMethod);

            var method = typeof(string).GetMethod(methodName, new[] { typeof(string) });

            if (method is not null)
                returnExpression = Expression.Call(expression1, method, constantExpression);
        }

        return returnExpression;
    }

    private static Expression GetCustomExpression(Expression body, ConstantExpression constantExpression,
        string methodName)
    {
        Expression returnExpression = null;
        var method = typeof(string).GetMethod(methodName, new[] { typeof(string) });

        if (method is not null) returnExpression = Expression.Call(body, method, constantExpression);
        return returnExpression;
    }

    #region [ Private Methods ]

    private object ParseValue(string value, Type type)
    {
        if (string.IsNullOrWhiteSpace(value) || value == DefaultValue) return GetDefaultValue(type);
        if (type.IsEnum)
            return Enum.Parse(type, value);
        if (type == typeof(Guid))
            return Guid.Parse(value);
        return TypeDescriptor.GetConverter(type).ConvertFrom(value);
    }

    private List<PropertyInfo> GetNestedProp(string name, Type obj)
    {
        var infos = new List<PropertyInfo>();
        foreach (var part in name.Split('.'))
        {
            if (obj == null) return null;

            if (obj.IsGenericType && obj.GetGenericTypeDefinition().GetInterfaces()
                    .Any(i => i.IsAssignableFrom(typeof(IEnumerable<>))))
            {
                if (SupportedEnumerableProperties.GetOperators().Contains(part.ToLower()))
                    ApplyToEnumerable = false;
                else
                    obj = obj.GetGenericArguments().FirstOrDefault();
            }

            if (obj != null)
            {
                var info = obj.GetProperty(part,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.GetProperty |
                    BindingFlags.Instance | BindingFlags.GetField);
                if (info == null) return infos;

                obj = info.PropertyType;
                infos.Add(info);
            }
        }

        return infos;
    }

    #endregion
}