using System;
using System.Collections.Generic;
using System.Linq;

namespace FultonSearch
{
    public class DocumentCandidate : IDocumentCandidate
    {
        private readonly List<string> terms;

        public DocumentCandidate(List<string> terms)
        {
            this.terms = terms;
        }

        public bool Matches(string term)
        {
            return terms.Contains(term);
        }

        public bool Matches(string term, double boost)
        {
            return terms.Contains(term);
        }

        public bool MatchesAll(string[] terms)
        {
            return terms.All(t => this.terms.Contains(t));
        }

        public bool MatchesAll(string[] terms, double boost)
        {
            return terms.All(t => this.terms.Contains(t));
        }

        public bool MatchesAny(string[] terms)
        {
            return terms.Any(t => this.terms.Contains(t));
        }

        public bool MatchesAny(string[] terms, double boost)
        {
            return terms.Any(t => this.terms.Contains(t));
        }
    }
}
