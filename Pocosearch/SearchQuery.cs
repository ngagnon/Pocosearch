using System;
using System.Collections.Generic;

namespace Pocosearch
{
    public class SearchQuery
    {
        public string SearchString { get; set; }
        public int Limit { get; set; } = 10;
        public int Fuzziness { get; set; } = Pocosearch.Fuzziness.Auto;
        public List<Source> Sources { get; set; }
    }

    public static class Fuzziness
    {
        public static readonly int Auto = -1;
        public static readonly int Off = 0;
    }

    public abstract class Source
    {}

    public class Source<TDocument> : Source
    {
        public List<Field<TDocument>> Fields { get; set; } = null;
    }

    public class Field<TDocument>
    {
        /* @TODO: pass a getter (x => x.MyField) instead of magic string */
        //public Func<TDocument, string> Getter { get; set; }
        public string Name { get; set; }
        public double Boost { get; set; } = 1.0;
        public bool Exclude { get; set; } = false;

        public Field(string name)
        {
            Name = name;
        }
    }
}