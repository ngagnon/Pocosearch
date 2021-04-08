using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Pocosearch.Internals;

namespace Pocosearch
{
    public interface IPocosearchClient
    {
        void SetupIndex<TDocument>();
        void DeleteIndex<TDocument>();
        void AddOrUpdate<TDocument>(TDocument document);
        void BulkAddOrUpdate<TDocument>(IEnumerable<TDocument> documents);
        void Remove<TDocument>(Guid documentId);
        void Remove<TDocument>(int documentId);
        void Remove<TDocument>(long documentId);
        void Remove<TDocument>(string documentId);
        IEnumerable<SearchResult> Search(SearchQuery query);
        IEnumerable<SearchResultCollection> MultiSearch(IEnumerable<SearchQuery> queries);
        IEnumerable<SearchResultCollection> MultiSearch(params SearchQuery[] queries);
        void Refresh<TDocument>();

        Task SetupIndexAsync<TDocument>();
        Task DeleteIndexAsync<TDocument>();
        Task AddOrUpdateAsync<TDocument>(TDocument document);
        Task BulkAddOrUpdateAsync<TDocument>(IEnumerable<TDocument> documents);
        Task RemoveAsync<TDocument>(Guid documentId);
        Task RemoveAsync<TDocument>(int documentId);
        Task RemoveAsync<TDocument>(long documentId);
        Task RemoveAsync<TDocument>(string documentId);
        Task<IEnumerable<SearchResult>> SearchAsync(SearchQuery query);
        Task<IEnumerable<SearchResultCollection>> MultiSearchAsync(IEnumerable<SearchQuery> queries);
        Task<IEnumerable<SearchResultCollection>> MultiSearchAsync(params SearchQuery[] queries);
        Task RefreshAsync<TDocument>();
    }
}