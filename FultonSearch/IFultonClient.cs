using System;
using System.Collections.Generic;

namespace FultonSearch
{
    public interface IFultonClient : IDisposable
    {
        void Scan<TIndex>() where TIndex : Index;

        void AddOrUpdate<TIndex>(int docId, string content) where TIndex :  Index;

        bool TryRemove<TIndex>(int docId) where TIndex : Index;

        IEnumerable<SearchResult> Search<TIndex>(string query) where TIndex : Index;
        
        IEnumerable<SearchResult> Search<TIndex>(FullTextQuery<TIndex> query) where TIndex : Index;

        IEnumerable<SearchResult> Search(CompoundQuery query);
    }
}
