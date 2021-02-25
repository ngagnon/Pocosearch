using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Pocosearch.Internals
{
    public static class ReflectionHelper
    {
        public static MemberInfo ResolveProperty(LambdaExpression lambdaExpression)
        {
            Expression expression = lambdaExpression.Body;

            for (;;)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        var memberExpression = (MemberExpression)expression;

                        if (memberExpression.Expression.NodeType != ExpressionType.Parameter)
                        {
                            throw new ArgumentException($"Expression '{lambdaExpression}' must resolve to a top-level member.", nameof(lambdaExpression));
                        }

                        return memberExpression.Member;
                    default:
                        throw new ArgumentException($"Expression '{lambdaExpression}' must resolve to a top-level member.", nameof(lambdaExpression));
                }
            }
        }
    }
}