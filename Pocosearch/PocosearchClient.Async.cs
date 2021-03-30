using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace Pocosearch
{
    public partial class PocosearchClient
    {
        public async Task SetupIndexAsync<TDocument>()
        {
            var indexName = GetIndexName<TDocument>();

            await indexManager
                .SetupIndexAsync<TDocument>(indexName)
                .ConfigureAwait(false);
        }

        public async Task DeleteIndexAsync<TDocument>()
        {
            var indexName = GetIndexName<TDocument>();
            var response = await elasticClient.Indices
                .DeleteAsync<StringResponse>(indexName)
                .ConfigureAwait(false);

            if (!response.Success)
                throw new ApiException(response);
        }

        public async Task AddOrUpdateAsync<TDocument>(TDocument document)
        {
            var indexName = GetIndexName<TDocument>();
            await indexManager.SetupIndexAsync<TDocument>(indexName).ConfigureAwait(false);

            var id = documentIdProvider.GetDocumentId(document);
            var serialized = pocoManager.Serialize(document);

            var response = await elasticClient
                .IndexAsync<StringResponse>(indexName, id, PostData.Serializable(serialized))
                .ConfigureAwait(false);

            if (!response.Success)
                throw new ApiException(response);
        }

        public async Task BulkAddOrUpdateAsync<TDocument>(IEnumerable<TDocument> documents)
        {
            var indexName = GetIndexName<TDocument>();
            await indexManager.SetupIndexAsync<TDocument>(indexName).ConfigureAwait(false);

            var bulkUpdate = PrepareBulkUpdateQuery<TDocument>(indexName, documents);
            var response = await elasticClient
                .BulkAsync<StringResponse>(indexName, bulkUpdate)
                .ConfigureAwait(false);

            if (!response.Success)
                throw new ApiException(response);
        }

        public Task RemoveAsync<TDocument>(Guid documentId)
        {
            return RemoveAsync<TDocument>(documentId.ToString());
        }

        public Task RemoveAsync<TDocument>(int documentId)
        {
            return RemoveAsync<TDocument>(documentId.ToString());
        }

        public async Task RemoveAsync<TDocument>(long documentId)
        {
            await RemoveAsync<TDocument>(documentId.ToString()).ConfigureAwait(false);
        }

        public async Task RemoveAsync<TDocument>(string documentId)
        {
            var indexName = GetIndexName<TDocument>();
            var response = await elasticClient
                .DeleteAsync<StringResponse>(indexName, documentId)
                .ConfigureAwait(false);

            if (!response.Success)
                throw new ApiException(response);
        }

        public async Task<IEnumerable<SearchResult>> SearchAsync(SearchQuery query)
        {
            var elasticQuery = searchQueryBuilder.Build(query); 
            var indexList = searchQueryBuilder.GetIndexNamesCSV(query);

            var searchResponse = await elasticClient
                .SearchAsync<StringResponse>(indexList, PostData.Serializable(elasticQuery))
                .ConfigureAwait(false);

            if (!searchResponse.Success)
                throw new ApiException(searchResponse);

            var body = searchResponse.Body;

            return searchResponseParser.Parse(body, query);
        }

        public async Task<IEnumerable<SearchResultCollection>> MultiSearchAsync(IEnumerable<SearchQuery> queries)
        {
            var queryList = queries.ToList();
            var request = PrepareMultiSearchQuery(queryList);
            var searchResponse = await elasticClient
                .MultiSearchAsync<StringResponse>(request)
                .ConfigureAwait(false);

            if (!searchResponse.Success)
                throw new ApiException(searchResponse);

            return ParseMultiSearchResponse(searchResponse.Body, queryList);
        }

        public async Task RefreshAsync<TDocument>()
        {
            var indexName = GetIndexName<TDocument>();
            var response = await elasticClient.Indices
                .RefreshAsync<StringResponse>(indexName)
                .ConfigureAwait(false);

            if (!response.Success)
                throw new ApiException(response);
        }
    }
}
 