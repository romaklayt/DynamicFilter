using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using romaklayt.DynamicFilter.Common;

namespace romaklayt.DynamicFilter.Parser
{
    public static class Extensions
    {
        public static ExpressionDynamicFilter<T> BindFilterExpressions<T>(this BaseDynamicFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var model = Activator.CreateInstance(typeof(ExpressionDynamicFilter<T>));

            var itemType = typeof(ExpressionDynamicFilter<T>).GenericTypeArguments[0];

            var parameter = Expression.Parameter(itemType, "x");

            ExtractFilters(model, filter, parameter, itemType);

            ExtractOrder(model, filter, parameter);

            ExtractPagination(model, filter);

            ExtractSelect(model, filter, parameter, itemType);
            return model as ExpressionDynamicFilter<T>;
        }
        
        public static PageModel<T> ToPagedList<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (pageSize <= decimal.Zero) pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PageModel<T>(items, count, pageNumber, pageSize);
        }

        public static PageModel<T> ToPagedList<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
        {
            if (pageSize <= decimal.Zero) pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var enumerable = source.ToList();
            var count = enumerable.Count();
            var items = enumerable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PageModel<T>(items, count, pageNumber, pageSize);
        }

        public static async Task<PageModel<T>> ToPagedList<T>(this IAsyncEnumerable<T> source, int pageNumber,
            int pageSize)
        {
            if (pageSize <= decimal.Zero) pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var enumerable = await source.ToListAsync();
            var count = enumerable.Count();
            var items = enumerable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PageModel<T>(items, count, pageNumber, pageSize);
        }
        
        private static Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync();

            async Task<List<T>> ExecuteAsync()
            {
                var list = new List<T>();

                await foreach (var element in source)
                {
                    list.Add(element);
                }

                return list;
            }
        }
        
        private static void ExtractPagination(object model, object bindingContext)
        {
            var page = bindingContext.GetType().GetProperty("Page").GetValue(bindingContext,null) as string;
            var pageSize = bindingContext.GetType().GetProperty("PageSize").GetValue(bindingContext,null) as string;

            if (!string.IsNullOrWhiteSpace(page))
                model.GetType().GetProperty("Page").SetValue(model, int.Parse(page));

            if (!string.IsNullOrWhiteSpace(pageSize))
                model.GetType().GetProperty("PageSize").SetValue(model, int.Parse(pageSize));
        }

        private static void ExtractSelect(object model, object bindingContext, ParameterExpression parameter, Type itemType)
        {
            var select = bindingContext.GetType().GetProperty("Select").GetValue(bindingContext,null) as string;

            if (!string.IsNullOrWhiteSpace(select))
            {
                model.GetType().GetProperty("SelectText").SetValue(model, select);
                var selectFields = select.Split(',');

                // new statement "new Data()"
                var xNew = Expression.New(itemType);

                // create initializers
                var bindings = selectFields.Select(o => o.Trim())
                    .Select(o =>
                        {

                            // property "Field1"
                            var mi = itemType.GetProperty(o, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance);

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

                model.GetType().GetProperty("Select").SetValue(model, lambda);
            }
        }


        private static void ExtractOrder(object model, object bindingContext, ParameterExpression parameter)
        {
            var order = bindingContext.GetType().GetProperty("Order").GetValue(bindingContext,null) as string;

            if (!string.IsNullOrWhiteSpace(order))
            {
                var orderItems = order.Split('=');
                if (orderItems.Count() > 1)
                {
                    model.GetType().GetProperty("OrderType").SetValue(model, Enum.Parse(typeof(OrderType), orderItems[1], true));
                    order = orderItems[0];
                }
                else
                    model.GetType().GetProperty("OrderType").SetValue(model, OrderType.Desc);

                var property = Expression.PropertyOrField(parameter, order);

                var orderExp = Expression.Lambda(Expression.Convert(property, typeof(Object)).Reduce(), parameter);

                model.GetType().GetProperty("Order").SetValue(model, orderExp);
            }
        }

        private static void ExtractFilters(object model, object bindingContext, ParameterExpression parameter, Type itemType)
        {
            var filter = bindingContext.GetType().GetProperty("Filter").GetValue(bindingContext,null) as string;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var filterAndValues = filter.Split(',').ToArray();

                LambdaExpression finalExpression = null;
                Expression currentExpression = null;
                var item = Activator.CreateInstance(itemType, true);

                for (int i = 0; i < filterAndValues.Count(); i++)
                {
                    if (filterAndValues[i].Contains('|'))
                    {
                        var orExpression = new ExpressionParser();
                        var filterAndValue = orExpression.DefineOperation(filterAndValues[i], itemType);
                        var options = filterAndValue[1].Split('|');

                        for (int j = 0; j < options.Count(); j++)
                        {
                            var expression = GetExpression(parameter, itemType, $"{filterAndValue[0]}{orExpression.GetOperation()}{options[j]}");

                            if (j == 0)
                            {
                                if (currentExpression == null)
                                    currentExpression = expression;
                                else
                                    currentExpression = Expression.And(currentExpression, expression);
                            }
                            else
                            {
                                currentExpression = Expression.Or(currentExpression, expression);
                            }

                        }
                    }
                    else
                    {
                        Expression expression = GetExpression(parameter, itemType, filterAndValues[i]);

                        if (currentExpression == null)
                        {
                            currentExpression = expression;
                        }
                        else
                        {
                            currentExpression = Expression.And(currentExpression, expression);
                        }
                    }
                }

                finalExpression = Expression.Lambda(currentExpression, parameter);

                model.GetType().GetProperty("Filter").SetValue(model, finalExpression);
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