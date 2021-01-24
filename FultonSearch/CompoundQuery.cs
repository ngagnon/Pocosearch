using System;

namespace FultonSearch
{
    public class CompoundQuery : IQuery
    {
        public IQuery[] Queries { get; }
        public int MinimumMatch { get; }

        private CompoundQuery(IQuery[] queries, int minimumMatch)
        {
            Queries = queries;
            MinimumMatch = minimumMatch;
        }

        public static CompoundQuery MatchAll(params IQuery[] queries)
        {
            return new CompoundQuery(queries, queries.Length);
        }
        
        public static CompoundQuery MatchAny(params IQuery[] queries)
        { 
            return new CompoundQuery(queries, 1);
        }

        public static CompoundQuery MatchAny(int n, IQuery[] queries)
        {
            if (n > queries.Length)
                throw new ArgumentException(nameof(n));

            return new CompoundQuery(queries, n);
        }
    }
}
