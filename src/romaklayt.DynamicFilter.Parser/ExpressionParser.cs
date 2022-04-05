using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Common.Exceptions;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Parser;

public class ExpressionParser
{
    public ExpressionParser(string filterValues, Type itemType)
    {
        Properties = new List<PropertyInfo>();

        var values = DefineOperation(filterValues, itemType);

        Value = ParseValue(values[1]);
    }


    #region [ Public methods ]

    public Expression GetExpression(ParameterExpression parameter)
    {
        var constantExpression = Expression.Constant(Value);
        Expression returnExpression = null;
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

        Expression body = parameter;
        foreach (var member in Properties)
            if (member.PropertyType.IsGenericType && member.PropertyType.GetGenericTypeDefinition().GetInterfaces()
                    .Any(i => i.IsAssignableFrom(typeof(IEnumerable<>))))
            {
                genericType = member.PropertyType;
                baseExp = Expression.Property(body, member);
                body = Expression.Property(body, member);
            }
            else
            {
                if (genericType?.GetGenericArguments().FirstOrDefault()?.FullName == member.DeclaringType?.FullName)
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

        switch (Condition)
        {
            default:
            case OperatorEnum.Equals:
                returnExpression = Expression.Equal(body, constantExpression);
                break;
            case OperatorEnum.ContainsCaseInsensitive:
                returnExpression = GetCustomCaseInsensitiveExpression(body, "Contains");
                break;
            case OperatorEnum.Contains:
                returnExpression = GetCustomExpression(body, constantExpression, "Contains");
                break;
            case OperatorEnum.GreaterThan:
                returnExpression = Expression.GreaterThan(body, constantExpression);
                break;
            case OperatorEnum.LessThan:
                returnExpression = Expression.LessThan(body, constantExpression);
                break;
            case OperatorEnum.GreaterOrEqual:
                returnExpression = Expression.GreaterThanOrEqual(body, constantExpression);
                break;
            case OperatorEnum.LessOrEqual:
                returnExpression = Expression.LessThanOrEqual(body, constantExpression);
                break;
            case OperatorEnum.StartsWith:
                returnExpression = GetCustomExpression(body, constantExpression, "StartsWith");
                break;
            case OperatorEnum.EndsWith:
                returnExpression = GetCustomExpression(body, constantExpression, "EndsWith");
                break;
            case OperatorEnum.StartsWithCaseInsensitive:
                returnExpression = GetCustomCaseInsensitiveExpression(body, "StartsWith");
                break;
            case OperatorEnum.EndsWithCaseInsensitive:
                returnExpression = GetCustomCaseInsensitiveExpression(body, "EndsWith");
                break;
            case OperatorEnum.EqualsCaseInsensitive:
                returnExpression = GetCustomCaseInsensitiveExpression(body, "Equals");
                break;
        }

        if (IsNotExpression) returnExpression = Expression.Not(returnExpression);

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

    private Expression GetCustomCaseInsensitiveExpression(Expression body, string methodName)
    {
        Expression returnExpression = null;
        var constantExpression = Expression.Constant(Value.ToString().ToLower());

        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

        if (toLowerMethod is not null)
        {
            var expression1 = Expression.Call(body, toLowerMethod);

            var method = typeof(string).GetMethod(methodName, new[] {typeof(string)});

            if (method is not null)
                returnExpression = Expression.Call(expression1, method, constantExpression);
        }

        return returnExpression;
    }

    private static Expression GetCustomExpression(Expression body, ConstantExpression constantExpression, string methodName)
    {
        Expression returnExpression = null;
        var method = typeof(string).GetMethod(methodName, new[] {typeof(string)});

        if (method is not null) returnExpression = Expression.Call(body, method, constantExpression);
        return returnExpression;
    }

    #endregion

    private object GetDefaultValue(Type type)
    {
        if (type == null) throw new ArgumentNullException("type");

        var e = Expression.Lambda<Func<object>>(
            Expression.Convert(
                Expression.Default(type), typeof(object)
            )
        );

        return e.Compile()();
    }

    #region [ Properties ]

    private List<PropertyInfo> Properties { get; set; }
    private object Value { get; set; }
    private OperatorEnum Condition { get; set; }
    private bool IsNotExpression { get; set; }

    #endregion

    #region [ Private Methods ]

    private object ParseValue(string value)
    {
        object parsedValue = null;

        parsedValue = ChangeType(value, Properties.LastOrDefault()?.PropertyType);

        return parsedValue;
    }

    private object ChangeType(string value, Type type)
    {
        if (string.IsNullOrWhiteSpace(value)) return GetDefaultValue(type);
        if (type.IsEnum)
            return Enum.Parse(type, value);
        if (type == typeof(Guid))
            return Guid.Parse(value);
        var converter = TypeDescriptor.GetConverter(type);

        return converter.ConvertFrom(value);
    }


    private string[] DefineOperation(string filterValues, Type itemType)
    {
        string[] values = null;
        
        foreach (OperatorEnum operatorName in Enum.GetValues(typeof(OperatorEnum)))
        {
            var op = typeof(Operators).GetField(operatorName.ToString()).GetValue(new Operators()).ToString();
            if (!filterValues.Contains(op)) continue;
            values = filterValues.Split(new []{op},StringSplitOptions.RemoveEmptyEntries);
            Condition = operatorName;
            if (!values[0].EndsWith("!")) continue;
            IsNotExpression = true;
            values[0] = values[0].TrimEnd('!');
        }

        if (values == null)
            throw new ArgumentNullException("filter");

        var property = itemType.GetProperty(values[0],
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty |
            BindingFlags.Instance);

        if (property != null)
            Properties.Add(property);
        else
            Properties = GetNestedProp(values[0], itemType);

        if (Properties == null || !Properties.Any())
            throw new PropertyNotFoundException(values[0], itemType.Name);

        return values;
    }

    private List<PropertyInfo> GetNestedProp(string name, Type obj)
    {
        var infos = new List<PropertyInfo>();
        foreach (var part in name.Split('.'))
        {
            if (obj == null) return null;

            if (obj.IsGenericType && obj.GetGenericTypeDefinition().GetInterfaces()
                    .Any(i => i.IsAssignableFrom(typeof(IEnumerable<>))))
                obj = obj.GetGenericArguments().FirstOrDefault();
            if (obj != null)
            {
                var info = obj.GetProperty(part,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.GetProperty |
                    BindingFlags.Instance);
                if (info == null) return infos;

                obj = info.PropertyType;
                infos.Add(info);
            }
        }

        return infos;
    }

    #endregion
}