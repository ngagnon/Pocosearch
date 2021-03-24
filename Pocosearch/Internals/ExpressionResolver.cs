using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Pocosearch.Internals
{
    public static class ExpressionResolver
    {
        public static Filter ResolvePredicate(LambdaExpression lambdaExpression)
        {
            var lambdaBody = lambdaExpression.Body;
            return ResolvePredicateExpression(lambdaBody);
        }

        private static Filter ResolvePredicateExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    return ResolveLogicalCombination((BinaryExpression)expression);

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    return ResolveComparison((BinaryExpression)expression);

                default:
                    throw new ArgumentException($"Unexpected expression '{expression}'");
            }
        }

        private static Filter ResolveComparison(BinaryExpression expression)
        {
            MemberInfo memberInfo;
            Expression valueExpression;

            if (IsPropertyAccess(expression.Left))
            {
                memberInfo = ((MemberExpression)expression.Left).Member;
                valueExpression = expression.Right;
            }
            else if (IsPropertyAccess(expression.Right))
            {
                memberInfo = ((MemberExpression)expression.Right).Member;
                valueExpression = expression.Left;
            }
            else
            {
                throw new ArgumentException($"Unexpected expression '{expression}'");
            }

            var value = Expression.Lambda<Func<object>>(Expression.Convert(valueExpression, typeof(object))).Compile();

            return new ComparisonFilter
            {
                PropertyName = memberInfo.Name,
                ComparisonType = GetComparisonType(expression.NodeType),
                Value = value()
            };
        }

        private static ComparisonType GetComparisonType(ExpressionType expressionType)
        {
            return expressionType switch
            {
                ExpressionType.Equal => ComparisonType.Equal,
                ExpressionType.NotEqual => ComparisonType.NotEqual,
                ExpressionType.LessThan => ComparisonType.LessThan,
                ExpressionType.LessThanOrEqual => ComparisonType.LessThanOrEqual,
                ExpressionType.GreaterThan => ComparisonType.GreaterThan,
                ExpressionType.GreaterThanOrEqual => ComparisonType.GreaterThanOrEqual,
                _ => throw new ArgumentException($"Unexpected expression type '{expressionType}'")
            };
        }

        private static bool IsPropertyAccess(Expression expression)
        {
            var memberExpression = expression as MemberExpression;

            return memberExpression != null
                && memberExpression.Expression.NodeType == ExpressionType.Parameter;
        }

        private static Filter ResolveLogicalCombination(BinaryExpression expression)
        {
            return new FilterCombination
            {
                CombinationType = expression.NodeType == ExpressionType.AndAlso
                    ? CombinationType.MatchAll
                    : CombinationType.MatchAny,
                Filters = new List<Filter> 
                {
                    ResolvePredicateExpression(expression.Left),
                    ResolvePredicateExpression(expression.Right),
                }
            };
        }

        public static MemberInfo ResolveProperty(LambdaExpression lambdaExpression)
        {
            var lambdaBody = lambdaExpression.Body;

            if (lambdaBody.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException($"Expression '{lambdaExpression}' does not match the expected format x => x.SomeProperty.", nameof(lambdaExpression));

            var memberExpression = (MemberExpression)lambdaBody;

            if (memberExpression.Expression.NodeType != ExpressionType.Parameter)
                throw new ArgumentException($"Expression '{lambdaExpression}'  does not match the expected format x => x.SomeProperty.", nameof(lambdaExpression));

            return memberExpression.Member;
        }
    }
}