using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Parser.Models;

namespace romaklayt.DynamicFilter.Parser
{
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

            if (!string.IsNullOrWhiteSpace(page))
                model.GetType().GetProperty("Page")?.SetValue(model, int.Parse(page));

            if (!string.IsNullOrWhiteSpace(pageSize))
                model.GetType().GetProperty("PageSize")?.SetValue(model, int.Parse(pageSize));
        }

        internal static void ExtractSelect<TSource, TTarget>(object model, object bindingContext)
        {
            var select = bindingContext.GetType().GetProperty("Select")?.GetValue(bindingContext, null) as string;

            if (!string.IsNullOrWhiteSpace(select))
                model.GetType().GetProperty("Select")
                    ?.SetValue(model, BuildSelector<TSource, TTarget>(select));
        }

        private static void ExtractOrder(object model, object bindingContext, ParameterExpression parameter)
        {
            var order = bindingContext.GetType().GetProperty("Order")?.GetValue(bindingContext, null) as string;

            if (string.IsNullOrWhiteSpace(order)) return;
            var orderItems = order.Split('=');
            if (orderItems.Count() > 1)
            {
                model.GetType().GetProperty("OrderType")
                    ?.SetValue(model, Enum.Parse(typeof(OrderType), orderItems[1], true));
                order = orderItems[0];
            }
            else
            {
                model.GetType().GetProperty("OrderType")?.SetValue(model, OrderType.Asc);
            }

            var property = Expression.PropertyOrField(parameter, order);

            var orderExp = Expression.Lambda(Expression.Convert(property, typeof(object)).Reduce(), parameter);

            model.GetType().GetProperty("Order")?.SetValue(model, orderExp);
        }

        internal static void ExtractFilters(object model, object bindingContext, ParameterExpression parameter,
            Type itemType)
        {
            var filter = bindingContext.GetType().GetProperty("Filter")?.GetValue(bindingContext, null) as string;

            if (string.IsNullOrWhiteSpace(filter)) return;
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
                .ToList();
            var u = array.GroupBy(strings => strings[0]).ToList();
            foreach (var member in u)
            {
                BuildSelectorExpression(typeof(TTarget), parameter, member, out var list);
                body.AddRange(list);
            }

            return Expression.Lambda<Func<TSource, TTarget>>(
                Expression.MemberInit(Expression.New(typeof(TTarget)), body), parameter);
        }

        private static Expression BuildSelectorExpression(Type targetType, Expression source,
            IGrouping<string, string[]> allMembers, out List<MemberBinding> bindings, int depth = 0)
        {
            bindings = new List<MemberBinding>();
            foreach (var members in allMembers.GroupBy(strings => strings[depth]).ToList())
            {
                var memberName = members.ToArray()[0][depth];
                var target = Expression.Constant(null, targetType);
                var targetMember = Expression.PropertyOrField(target, memberName);
                var sourceMember = Expression.PropertyOrField(source, memberName);
                var childMembers = members.Where(path => depth + 1 < path.Count()).ToArray();
                if (!childMembers.Any())
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
                            members, out _, depth + 1);
                        targetValue = Expression.Call(typeof(Enumerable), nameof(Enumerable.Select),
                            new[] { sourceElementType, targetElementType }, sourceMember,
                            Expression.Lambda(targetValue, sourceElementParam));

                        targetValue = CorrectEnumerableResult(targetValue, targetElementType, targetMember.Type);
                    }
                    else
                    {
                        targetValue = Expression.Condition(
                            Expression.Equal(Expression.Property(source, memberName),
                                Expression.Constant(null, sourceMember.Type)),
                            Expression.Constant(null, sourceMember.Type), BuildSelectorExpression(targetMember.Type,
                                sourceMember,
                                members, out _, depth + 1));
                    }
                    bindings.Add(Expression.Bind(targetMember.Member, targetValue));
                }
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
}