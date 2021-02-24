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
        public List<Field> Fields { get; set; } = new List<Field>();
    }

    public class Source<TDocument> : Source
    {
        public override Type DocumentType => typeof(TDocument);

        public Source<TDocument> Exclude<TMember>(Expression<Func<TDocument, TMember>> member)
        {
            var memberInfo = ReflectionHelper.ResolveProperty(member);
            return Exclude(memberInfo.Name);
        }

        public Source<TDocument> Exclude(string memberName)
        {
            var field = Fields.Find(x => x.Name == memberName);

            if (field == null)
            {
                field = new Field(memberName);
                Fields.Add(field);
            }

            field.Exclude = true;

            return this;
        }
    }

    public class Field
    {
        public string Name { get; set; }
        public double? Boost { get; set; } = null;
        public bool Exclude { get; set; } = false;

        public Field(string name)
        {
            Name = name;
        }
    }
}