using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Parser
{
    public class ExpressionParser
    {
        public ExpressionParser(string filterValues, Type itemType)
        {
            Properties = new List<PropertyInfo>();

            var values = DefineOperation(filterValues, itemType);

            Value = ParseValue(values[1]);
        }

        public ExpressionParser()
        {
            Properties = new List<PropertyInfo>();
        }

        private object ChangeType(object value, Type type)
        {
            if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
            if (value == null) return null;
            if (type == value.GetType()) return value;
            if (type.IsEnum)
            {
                if (value is string s)
                    return Enum.Parse(type, s);
                return Enum.ToObject(type, value);
            }

            if (!type.IsInterface && type.IsGenericType)
            {
                var innerType = type.GetGenericArguments()[0];
                var innerValue = ChangeType(value, innerType);
                return Activator.CreateInstance(type, innerValue);
            }

            if (value is string && type == typeof(Guid)) return new Guid(value as string);
            if (value is string && type == typeof(Version)) return new Version(value as string);
            if (!(value is IConvertible)) return value;
            return ChangeType(value, type);
        }

        #region [ Public methods ]

        public Expression GetExpression(ParameterExpression parameter)
        {
            var constantExpression = Expression.Constant(Value);
            Expression returnExpression = null;
            ParameterExpression subParam = null;
            Expression baseExp = null;
            Type genericType = null;

            if (Nullable.GetUnderlyingType(Properties.LastOrDefault().PropertyType) != null)
            {
                var type = typeof(Nullable<>).MakeGenericType(
                    Nullable.GetUnderlyingType(Properties.LastOrDefault().PropertyType));
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
                {
                    returnExpression = Expression.Equal(body, constantExpression);
                    break;
                }
                case OperatorEnum.Contains:
                {
                    constantExpression = Expression.Constant(Value.ToString().ToLower());

                    var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

                    if (toLowerMethod is not null)
                    {
                        var expression1 = Expression.Call(body, toLowerMethod);

                        var method = typeof(string).GetMethod("Contains", new[] {typeof(string)});

                        if (method is not null)
                            returnExpression = Expression.Call(expression1, method, constantExpression);
                    }

                    break;
                }
                case OperatorEnum.ContainsCaseSensitive:
                {
                    var method = typeof(string).GetMethod("Contains", new[] {typeof(string)});

                    if (method is not null) returnExpression = Expression.Call(body, method, constantExpression);

                    break;
                }
                case OperatorEnum.GreaterThan:
                {
                    returnExpression = Expression.GreaterThan(body, constantExpression);

                    break;
                }
                case OperatorEnum.LessThan:
                {
                    returnExpression = Expression.LessThan(body, constantExpression);
                    break;
                }
                case OperatorEnum.GreaterOrEqual:
                {
                    returnExpression = Expression.GreaterThanOrEqual(body, constantExpression);
                    break;
                }
                case OperatorEnum.LessOrEqual:
                {
                    returnExpression = Expression.LessThanOrEqual(body, constantExpression);
                    break;
                }
                case OperatorEnum.NotEquals:
                {
                    returnExpression = Expression.NotEqual(body, constantExpression);
                    break;
                }
            }

            if (genericType != null)
            {
                var anyMethod = typeof(Enumerable)
                    .GetMethods()
                    .FirstOrDefault(m => m.Name == "Any" && m.GetParameters().Count() == 2)
                    ?.MakeGenericMethod(genericType.GetGenericArguments().FirstOrDefault());


                if (constantExpression.Value != null && anyMethod is not null && returnExpression is not null)
                    returnExpression =
                        Expression.Call(anyMethod, baseExp, Expression.Lambda(returnExpression, subParam));
            }

            return returnExpression;
        }

        #endregion

        #region [ Properties ]

        public List<PropertyInfo> Properties { get; set; }
        public object Value { get; set; }
        public OperatorEnum Condition { get; set; }

        #endregion

        #region [ Private Methods ]

        private object ParseValue(string value)
        {
            object parsedValue = null;

            foreach (var property in Properties)
            {
                if (property.PropertyType.IsClass && property.PropertyType.Name.ToLower() != "string" &&
                    property.PropertyType.Name.ToLower() != "datetime")
                    continue;
                //Verifying if is nullable
                if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                    var type = typeof(Nullable<>).MakeGenericType(underlyingType);

                    object newValue;

                    if (underlyingType.IsEnum) newValue = Enum.Parse(underlyingType, value);

                    newValue = ChangeType(value, underlyingType);

                    var nullableObject = Activator.CreateInstance(type, newValue);

                    parsedValue = nullableObject;
                }
                else
                {
                    parsedValue = ChangeType(value, property.PropertyType);
                }
            }

            return parsedValue;
        }

        private object ChangeType(string value, Type type)
        {
            if (type.IsEnum)
                return ChangeType(Enum.Parse(type, value), type);

            if (type == typeof(Guid))
                return Guid.Parse(value);

            var converter = TypeDescriptor.GetConverter(type);

            return converter.ConvertFrom(value);
        }

        public string GetOperation()
        {
            switch (Condition)
            {
                case OperatorEnum.Equals:
                    return "=";
                case OperatorEnum.Contains:
                    return "%";
                case OperatorEnum.ContainsCaseSensitive:
                    return "%%";
                case OperatorEnum.GreaterThan:
                    return ">";
                case OperatorEnum.LessThan:
                    return "<";
                case OperatorEnum.GreaterOrEqual:
                    return ">=";
                case OperatorEnum.LessOrEqual:
                    return "<=";
                case OperatorEnum.NotEquals:
                    return "!=";
                default:
                    return "=";
            }
        }

        public string[] DefineOperation(string filterValues, Type itemType)
        {
            string[] values = null;

            if (filterValues.Contains('='))
            {
                values = filterValues.Split('=');
                Condition = OperatorEnum.Equals;
            }

            if (filterValues.Contains('%'))
            {
                if (filterValues.Contains("%%"))
                {
                    Condition = OperatorEnum.ContainsCaseSensitive;
                    values = Regex.Split(filterValues, "%%");
                }
                else
                {
                    Condition = OperatorEnum.Contains;
                    values = filterValues.Split('%');
                }
            }

            if (filterValues.Contains('>'))
            {
                values = filterValues.Split('>');
                Condition = OperatorEnum.GreaterThan;
            }

            if (filterValues.Contains('<'))
            {
                values = filterValues.Split('<');
                Condition = OperatorEnum.LessThan;
            }

            if (filterValues.Contains(">="))
            {
                values = Regex.Split(filterValues, ">=");
                Condition = OperatorEnum.GreaterOrEqual;
            }

            if (filterValues.Contains("<="))
            {
                values = Regex.Split(filterValues, "<=");
                Condition = OperatorEnum.LessOrEqual;
            }

            if (filterValues.Contains("!="))
            {
                values = Regex.Split(filterValues, "!=");
                Condition = OperatorEnum.NotEquals;
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

        public List<PropertyInfo> GetNestedProp(string name, Type obj)
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
}