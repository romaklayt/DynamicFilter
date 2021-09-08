using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Parser
{
    public static class DynamicModelParser
    {
        public static ExpressionDynamicFilter<T> BindFilterExpressions<T>(this BaseDynamicFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var model = Activator.CreateInstance(typeof(ExpressionDynamicFilter<T>));

            var itemType = typeof(ExpressionDynamicFilter<T>).GenericTypeArguments[0];

            var parameter = Expression.Parameter(itemType, "x");

            ExtractFilters(model, filter, parameter, itemType);

            ExtractOrder(model, filter, parameter);

            ExtractPagination(model, filter);

            ExtractSelect(model, filter, parameter, itemType);
            return model as ExpressionDynamicFilter<T>;
        }

        private static void ExtractPagination(object model, object bindingContext)
        {
            var page = bindingContext.GetType().GetProperty("Page")?.GetValue(bindingContext, null) as string;
            var pageSize = bindingContext.GetType().GetProperty("PageSize")?.GetValue(bindingContext, null) as string;

            if (!string.IsNullOrWhiteSpace(page))
                model.GetType().GetProperty("Page")?.SetValue(model, int.Parse(page));

            if (!string.IsNullOrWhiteSpace(pageSize))
                model.GetType().GetProperty("PageSize")?.SetValue(model, int.Parse(pageSize));
        }

        private static void ExtractSelect(object model, object bindingContext, ParameterExpression parameter,
            Type itemType)
        {
            var select = bindingContext.GetType().GetProperty("Select")?.GetValue(bindingContext, null) as string;

            if (!string.IsNullOrWhiteSpace(select))
            {
                model.GetType().GetProperty("SelectText")?.SetValue(model, select);
                var selectFields = select.Split(',');

                // new statement "new Data()"
                var xNew = Expression.New(itemType);

                // create initializers
                var bindings = selectFields.Select(o => o.Trim())
                    .Select(o =>
                        {
                            // property "Field1"
                            var mi = itemType.GetProperty(o,
                                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic |
                                BindingFlags.GetProperty | BindingFlags.Instance);

                            // original value "o.Field1"
                            var xOriginal = Expression.PropertyOrField(parameter, o);

                            // set value "Field1 = o.Field1"
                            return Expression.Bind(mi, xOriginal);
                        }
                    );

                // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
                var xInit = Expression.MemberInit(xNew, bindings);

                // expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
                var lambda = Expression.Lambda(xInit, parameter);

                model.GetType().GetProperty("Select")?.SetValue(model, lambda);
            }
        }


        private static void ExtractOrder(object model, object bindingContext, ParameterExpression parameter)
        {
            var order = bindingContext.GetType().GetProperty("Order")?.GetValue(bindingContext, null) as string;

            if (!string.IsNullOrWhiteSpace(order))
            {
                var orderItems = order.Split('=');
                if (orderItems.Count() > 1)
                {
                    model.GetType().GetProperty("OrderType")
                        ?.SetValue(model, Enum.Parse(typeof(OrderType), orderItems[1], true));
                    order = orderItems[0];
                }
                else
                {
                    model.GetType().GetProperty("OrderType")?.SetValue(model, OrderType.Desc);
                }

                var property = Expression.PropertyOrField(parameter, order);

                var orderExp = Expression.Lambda(Expression.Convert(property, typeof(object)).Reduce(), parameter);

                model.GetType().GetProperty("Order")?.SetValue(model, orderExp);
            }
        }

        private static void ExtractFilters(object model, object bindingContext, ParameterExpression parameter,
            Type itemType)
        {
            var filter = bindingContext.GetType().GetProperty("Filter")?.GetValue(bindingContext, null) as string;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var filterAndValues = filter.Split(',').ToArray();

                LambdaExpression finalExpression = null;
                Expression currentExpression = null;

                for (var i = 0; i < filterAndValues.Count(); i++)
                    if (filterAndValues[i].Contains('|'))
                    {
                        var orExpression = new ExpressionParser();
                        var filterAndValue = orExpression.DefineOperation(filterAndValues[i], itemType);
                        var options = filterAndValue[1].Split('|');

                        for (var j = 0; j < options.Count(); j++)
                        {
                            var expression = GetExpression(parameter, itemType,
                                $"{filterAndValue[0]}{orExpression.GetOperation()}{options[j]}");

                            if (j == 0)
                            {
                                currentExpression = currentExpression == null
                                    ? expression
                                    : Expression.And(currentExpression, expression);
                            }
                            else
                            {
                                if (currentExpression != null)
                                    currentExpression = Expression.Or(currentExpression, expression);
                            }
                        }
                    }
                    else
                    {
                        var expression = GetExpression(parameter, itemType, filterAndValues[i]);

                        if (currentExpression == null)
                            currentExpression = expression;
                        else
                            currentExpression = Expression.And(currentExpression, expression);
                    }

                if (currentExpression != null) finalExpression = Expression.Lambda(currentExpression, parameter);

                model.GetType().GetProperty("Filter")?.SetValue(model, finalExpression);
            }
        }


        private static Expression GetExpression(ParameterExpression parameter, Type itemType, string filterAndValue)
        {
            var expressionType = new ExpressionParser(filterAndValue, itemType);

            var expression = expressionType.GetExpression(parameter);
            return expression;
        }
    }
}