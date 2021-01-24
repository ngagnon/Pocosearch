using System;

namespace FultonSearch
{
    public interface IDocumentCandidate
    {
        bool Matches(string term);
        bool Matches(string term, double boost);

        bool MatchesAll(string[] terms);
        bool MatchesAll(string[] terms, double boost);

        bool MatchesAny(string[] terms);
        bool MatchesAny(string[] terms, double boost);
    }
}

