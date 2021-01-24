using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FultonSearch.Entities;

namespace FultonSearch.Core
{
    /// <summary>
    /// @TODO: write unit tests for this
    /// </summary>
    public static class PredicateParser
    {
        public static IEnumerable<SearchToken> ExtractTokens(Expression<Func<IDocumentCandidate, bool>> predicate)
        {
            return ExtractTokens(predicate.Body);
        }

        private static IEnumerable<SearchToken> ExtractTokens(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Call)
            {
                var methodCall = (MethodCallExpression)expression;

                if (IsValidMethodCall(methodCall))
                {
                    var tokens = ParseMethodCall(methodCall);

                    foreach (var token in tokens)
                        yield return token;
                }
                else
                {
                    throw new InvalidOperationException("Invalid predicate");
                }
            }
            else if (IsNotExpression(expression))
            {
                var unaryExpression = (UnaryExpression)expression;
                var tokens = ExtractTokens(unaryExpression.Operand);

                foreach (var token in tokens)
                    yield return token;
            }
            else if (IsBooleanExpression(expression))
            {
                var binaryExpression = (BinaryExpression)expression;
                var leftTokens = ExtractTokens(binaryExpression.Left);
                var rightTokens = ExtractTokens(binaryExpression.Right);

                foreach (var token in leftTokens.Concat(rightTokens))
                    yield return token;
            }
            else
            {
                throw new InvalidOperationException("Invalid predicate");
            }
        }

        private static bool IsValidMethodCall(MethodCallExpression methodCall)
        {
            return methodCall.Object?.Type == typeof(IDocumentCandidate)
                && methodCall.Object?.NodeType == ExpressionType.Parameter
                && methodCall.Arguments.Count > 0;
        }

        private static bool IsNotExpression(Expression expression)
        {
            return expression.NodeType == ExpressionType.Not;
        }

        private static bool IsBooleanExpression(Expression expression)
        {
            return expression.NodeType == ExpressionType.AndAlso
                || expression.NodeType == ExpressionType.OrElse;
        }

        private static IEnumerable<SearchToken> ParseMethodCall(MethodCallExpression methodCall)
        {
            if (methodCall.Method.Name == nameof(IDocumentCandidate.Matches))
            {
                var token = new SearchToken();

                token.Value = (string)Evaluate(methodCall.Arguments[0]);
                token.Boost = 1;

                if (methodCall.Arguments.Count >= 2 && methodCall.Arguments[1].Type == typeof(double))
                    token.Boost = (double)Evaluate(methodCall.Arguments[1]);

                yield return token;
            }
            else
            { 
                var boost = 1.0;

                if (methodCall.Arguments.Count >= 2 && methodCall.Arguments[1].Type == typeof(double))
                    boost = (double)Evaluate(methodCall.Arguments[1]);

                var tokens = (string[])Evaluate(methodCall.Arguments[0]);

                foreach (var token in tokens)
                    yield return new SearchToken { Value = token, Boost = boost };
            }
        }

        private static object Evaluate(Expression expression)
        {
            if (expression is ConstantExpression)
                return (expression as ConstantExpression).Value;

            if (expression is MemberExpression)
                return Evaluate(expression as MemberExpression);

            throw new InvalidOperationException("Invalid predicate");
        }

        private static object Evaluate(MemberExpression memberExpression)
        {
            var value = Evaluate(memberExpression.Expression);
            var member = memberExpression.Member;

            if (member is FieldInfo)
            {
                var fieldInfo = (FieldInfo)member;
                return fieldInfo.GetValue(value);
            }

            if (member is PropertyInfo)
            {
                var propertyInfo = (PropertyInfo)member;
                return propertyInfo.GetValue(value);
            }

            throw new InvalidOperationException("Invalid predicate");
        }
    }
}
