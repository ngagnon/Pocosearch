using System;
using System.Collections;
using System.Collections.Generic;

namespace Pocosearch
{
    public class SearchResult
    {
        public double Score { get; set; }
        public object Document { get; set; }
        public Type DocumentType { get; set; }
    }

    public class SearchResult<TDocument>
    {
        public double Score { get; set; }
        public TDocument Document { get; set; }
    }

    public class SearchResultCollection : IEnumerable<SearchResult>
    {
        private readonly IEnumerable<SearchResult> collection;

        public SearchResultCollection(IEnumerable<SearchResult> collection)
        {
            this.collection = collection;
        }

        public IEnumerator<SearchResult> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (collection as IEnumerable).GetEnumerator();
        }
    }
}