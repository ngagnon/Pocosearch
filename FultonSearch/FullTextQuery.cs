using System;
using System.Data;
using System.Linq.Expressions;

namespace FultonSearch
{
    public abstract class FullTextQuery : IQuery
    {
        /// <summary>
        /// Boolean expression that determines which documents match the query.
        /// e.g. x => x.Matches("hello") && (x.Matches("world") || x.Matches("everyone"))
        /// </summary>
        public Expression<Func<IDocumentCandidate, bool>> Predicate { get; set; }

        /// <summary>
        /// Optionally provide an SQL query to narrow down the subset of documents
        /// that need to be searched.
        ///
        /// This SQL query needs to SELECT a single field, the document IDs.
        /// </summary>
        public IDbCommand Subset { get; set; }

        /// <summary>
        /// Limit the number of search results.
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Rewards documents where a term appears more frequently.
        ///
        /// E.g. when searching for 'apple', a document where 'apple' appears 10
        /// times will score higher than a document where it only appears 3 times.
        /// </summary>
        public bool EnableTermFrequency { get; set; } = true;

        /// <summary>
        /// Rewards documents with fewer terms.
        ///
        /// E.g. a document of 10 words will score higher than a document with
        /// 1000 words.
        /// </summary>
        public bool EnableFieldLengthNorm { get; set; } = true;

        /// <summary>
        /// Rewards documents that contain a higher percentage of the query terms.
        ///
        /// E.g. when searching for 'quick brown fox', a document with both 'brown'
        /// and 'fox' will score higher than a document that only has 'fox'.
        /// </summary>
        public bool EnableCoordination { get; set; } = true;
    }

    public class FullTextQuery<TIndex> : FullTextQuery where TIndex : Index
    { 
    }
}
