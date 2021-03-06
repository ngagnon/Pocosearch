using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Pocosearch.Internals;

namespace Pocosearch
{
    public class SearchQuery
    {
        public string SearchString { get; set; }
        public int Limit { get; set; } = 10;
        public int Fuzziness { get; set; } = Pocosearch.Fuzziness.Auto;
        public List<Source> Sources { get; set; } = new List<Source>();

        public SearchQuery() {}

        public SearchQuery(string searchString)
        {
            SearchString = searchString;
        }

        public Source<TDocument> AddSource<TDocument>()
        {
            var source = new Source<TDocument>();
            Sources.Add(source);
            return source;
        }
    }

    public static class Fuzziness
    {
        public static readonly int Auto = -1;
        public static readonly int Off = 0;
    }

    public abstract class Source
    {
        public abstract Type DocumentType { get; }
        public Filter DocumentFilter { get; set; } = null;
        public List<Field> Fields { get; set; } = new List<Field>();
    }

    public class Source<TDocument> : Source
    {
        public override Type DocumentType => typeof(TDocument);

        public Source<TDocument> Filter(Expression<Func<TDocument, bool>> predicate)
        {
            DocumentFilter = ExpressionResolver.ResolvePredicate(predicate);
            return this;
        }

        public Source<TDocument> Configure<TMember>(Expression<Func<TDocument, TMember>> member, double boost = 1)
        {
            var memberInfo = ExpressionResolver.ResolveProperty(member);
            return Configure(memberInfo.Name, boost);
        }

        public Source<TDocument> Configure(string memberName, double boost = 1)
        {
            var field = GetField(memberName);
            field.Boost = boost;
            return this;
        }

        public Source<TDocument> Exclude<TMember>(Expression<Func<TDocument, TMember>> member)
        {
            var memberInfo = ExpressionResolver.ResolveProperty(member);
            return Exclude(memberInfo.Name);
        }

        public Source<TDocument> Exclude(string memberName)
        {
            var field = GetField(memberName);
            field.Exclude = true;
            return this;
        }

        private Field GetField(string name)
        {
            var field = Fields.Find(x => x.Name == name);

            if (field == null)
            {
                field = new Field(name);
                Fields.Add(field);
            }

            return field;
        }
    }

    public class Field
    {
        public string Name { get; set; }
        public double Boost { get; set; } = 1;
        public bool Exclude { get; set; } = false;

        public Field(string name)
        {
            Name = name;
        }
    }

    public abstract class Filter
    {}

    public class ComparisonFilter : Filter
    {
        public string PropertyName { get; set; }
        public ComparisonType ComparisonType { get; set; }
        public object Value { get; set; }
    }

    public enum ComparisonType
    {
        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual
    }

    public class FilterCombination : Filter
    {
        public List<Filter> Filters { get; set; }
        public CombinationType CombinationType { get; set; }
    }

    public enum CombinationType
    {
        MatchAll,
        MatchAny
    }
}