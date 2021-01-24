using System;
using System.Linq.Expressions;
using Xunit;
using FultonSearch.Core;
using Shouldly;
using System.Collections.Generic;
using FultonSearch.Entities;
using System.Linq;

namespace FultonSearch.Tests.Core
{
    public class PredicateParserFacts
    {
        [Fact]
        public void ExtractTokens_ExtractsTokensFromExpression()
        {
            Expression<Func<IDocumentCandidate, bool>> expr;
            expr = x => x.Matches("hello") && (x.Matches("world") || x.Matches("everyone", 2));

            var tokens = PredicateParser.ExtractTokens(expr).ToList();

            tokens.ShouldBe(new List<SearchToken>
            {
                new SearchToken { Value = "hello", Boost = 1.0 },
                new SearchToken { Value = "world", Boost = 1.0 },
                new SearchToken { Value = "everyone", Boost = 2.0 }
            });
        }

        [Fact]
        public void ExtractTokens_AcceptsVariablesInPredicate()
        {
            var word = "hello";

            Expression<Func<IDocumentCandidate, bool>> expr;
            expr = x => x.Matches(word) && (x.Matches("world") || x.Matches("everyone", 2));

            var tokens = PredicateParser.ExtractTokens(expr).ToList();

            tokens.ShouldBe(new List<SearchToken>
            {
                new SearchToken { Value = "hello", Boost = 1.0 },
                new SearchToken { Value = "world", Boost = 1.0 },
                new SearchToken { Value = "everyone", Boost = 2.0 }
            });
        }

        [Fact]
        public void ExtractTokens_AcceptsObjectPropertiesInPredicate()
        {
            var obj = new SomeClass { SomeString = "hello" };

            Expression<Func<IDocumentCandidate, bool>> expr;
            expr = x => x.Matches(obj.SomeString) && (x.Matches("world") || x.Matches("everyone", 2));

            var tokens = PredicateParser.ExtractTokens(expr).ToList();

            tokens.ShouldBe(new List<SearchToken>
            {
                new SearchToken { Value = "hello", Boost = 1.0 },
                new SearchToken { Value = "world", Boost = 1.0 },
                new SearchToken { Value = "everyone", Boost = 2.0 }
            });
        }

        [Fact]
        public void ExtractTokens_AcceptsMatchesAnyAndMatchesAllCalls()
        {
            var includes = new string[] { "hello", "world" };
            var excludes = new string[] { "bonjour", "monde" };

            Expression<Func<IDocumentCandidate, bool>> expr;
            expr = x => x.MatchesAll(includes) && !x.MatchesAny(excludes);

            var tokens = PredicateParser.ExtractTokens(expr).ToList();

            tokens.ShouldBe(new List<SearchToken>
            {
                new SearchToken { Value = "hello", Boost = 1.0 },
                new SearchToken { Value = "world", Boost = 1.0 },
                new SearchToken { Value = "bonjour", Boost = 1.0 },
                new SearchToken { Value = "monde", Boost = 1.0 }
            });
        }

        [Fact]
        public void ExtractTokens_ThrowsIfExpressionContainsExternalMethodCalls()
        {
            Expression<Func<IDocumentCandidate, bool>> expr;
            expr = x => ReturnsTrue(x);

            Should.Throw<InvalidOperationException>(() =>
                PredicateParser.ExtractTokens(expr).ToList()); 
        }

        [Fact]
        public void ExtractTokens_ThrowsIfExpressionUsesExternalCandidate()
        { 
            var doc = new DocumentCandidate(new List<string> { "hello", "world" });

            Expression<Func<IDocumentCandidate, bool>> expr;
            expr = x => doc.Matches("hello");

            Should.Throw<InvalidOperationException>(() =>
                PredicateParser.ExtractTokens(expr).ToList()); 
        }

        [Fact]
        public void ExtractTokens_ThrowsIfExpressionUsesExternalVariables()
        {
            var isTrue = true;

            Expression<Func<IDocumentCandidate, bool>> expr;
            expr = x => isTrue && x.Matches("hello");

            Should.Throw<InvalidOperationException>(() =>
                PredicateParser.ExtractTokens(expr).ToList()); 
        }

        private static bool ReturnsTrue(IDocumentCandidate doc)
        { 
            return true;
        }

        private class SomeClass
        { 
            public string SomeString { get; set; }
        }
    }
}
