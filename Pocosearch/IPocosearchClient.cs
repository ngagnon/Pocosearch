using System;
using System.Collections.Generic;
using System.Reflection;
using Elasticsearch.Net;
using Pocosearch.Internals;

namespace Pocosearch
{
    public interface IPocosearchClient
    {
        void SetupIndex<TDocument>();
        void AddOrUpdate<TDocument>(TDocument document);
        void BulkAddOrUpdate<TDocument>(IEnumerable<TDocument> documents);
        void Remove<TDocument>(Guid documentId);
        void Remove<TDocument>(int documentId);
        void Remove<TDocument>(long documentId);
        void Remove<TDocument>(string documentId);
        IEnumerable<SearchResult> Search(SearchQuery query);
        IEnumerable<SearchResultCollection> MultiSearch(IEnumerable<SearchQuery> queries);
    }
}