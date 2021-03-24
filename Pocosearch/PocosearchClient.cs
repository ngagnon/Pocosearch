using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Pocosearch.Internals;
using Pocosearch.Utils;

namespace Pocosearch
{
    public partial class PocosearchClient : IPocosearchClient
    {
        private readonly IElasticLowLevelClient elasticClient;
        private readonly DocumentIdProvider documentIdProvider;
        private readonly SearchIndexConfigurationProvider searchIndexProvider;
        private readonly SearchResponseParser searchResponseParser;
        private readonly SearchQueryBuilder searchQueryBuilder;
        private readonly IndexManager indexManager;
        private readonly PocoManager pocoManager;

        public PocosearchClient() : this(new ElasticLowLevelClient())
        {
        }

        public PocosearchClient(ConnectionConfiguration settings)
            : this(new ElasticLowLevelClient(settings))
        {
        }

        private PocosearchClient(IElasticLowLevelClient elasticClient)
        {
            this.elasticClient = elasticClient;
            documentIdProvider = new DocumentIdProvider();
            searchIndexProvider = new SearchIndexConfigurationProvider();
            pocoManager = new PocoManager();
            searchQueryBuilder = new SearchQueryBuilder(searchIndexProvider, pocoManager);
            searchResponseParser = new SearchResponseParser(searchIndexProvider, pocoManager);
            indexManager = new IndexManager(elasticClient, pocoManager);
        }

        public void SetupIndex<TDocument>()
        {
            var indexName = GetIndexName<TDocument>();
            indexManager.SetupIndex<TDocument>(indexName);
        }

        public void AddOrUpdate<TDocument>(TDocument document)
        {
            var indexName = GetIndexName<TDocument>();
            var id = documentIdProvider.GetDocumentId(document);
            var serialized = pocoManager.Serialize(document);

            var response = elasticClient.Index<StringResponse>(indexName, id, PostData.Serializable(serialized));

            if (!response.Success)
                throw new ApiException(response);
        }

        public void BulkAddOrUpdate<TDocument>(IEnumerable<TDocument> documents)
        {
            var indexName = GetIndexName<TDocument>();
            var bulkUpdate = PrepareBulkUpdateQuery<TDocument>(indexName, documents);
            var response = elasticClient.Bulk<StringResponse>(indexName, bulkUpdate);

            if (!response.Success)
                throw new ApiException(response);
        }

        public void Remove<TDocument>(Guid documentId)
        {
            Remove<TDocument>(documentId.ToString());
        }

        public void Remove<TDocument>(int documentId)
        {
            Remove<TDocument>(documentId.ToString());
        }

        public void Remove<TDocument>(long documentId)
        {
            Remove<TDocument>(documentId.ToString());
        }

        public void Remove<TDocument>(string documentId)
        {
            var indexName = GetIndexName<TDocument>();
            var response = elasticClient.Delete<StringResponse>(indexName, documentId);

            if (!response.Success)
                throw new ApiException(response);
        }

        public IEnumerable<SearchResult> Search(SearchQuery query)
        {
            var elasticQuery = searchQueryBuilder.Build(query); 

            var searchResponse = elasticClient.Search<StringResponse>(
                PostData.Serializable(elasticQuery));

            if (!searchResponse.Success)
                throw new ApiException(searchResponse);

            var body = searchResponse.Body;

            return searchResponseParser.Parse(body, query);
        }

        public IEnumerable<SearchResultCollection> MultiSearch(IEnumerable<SearchQuery> queries)
        {
            var queryList = queries.ToList();
            var request = PrepareMultiSearchQuery(queryList);
            var searchResponse = elasticClient.MultiSearch<StringResponse>(request);

            if (!searchResponse.Success)
                throw new ApiException(searchResponse);

            return ParseMultiSearchResponse(searchResponse.Body, queryList);
        }

        private PostData PrepareMultiSearchQuery(List<SearchQuery> queryList)
        {
            var elasticQueries = queryList.Select(q => searchQueryBuilder.Build(q)); 
            var queryPackets = new List<object>();

            foreach (var query in elasticQueries)
            {
                queryPackets.Add(new {});
                queryPackets.Add(query);
            }

            return PostData.MultiJson(queryPackets);
        }

        private IEnumerable<SearchResultCollection> ParseMultiSearchResponse(string responseJson, List<SearchQuery> queryList)
        {
            using (var document = JsonDocument.Parse(responseJson))
            {
                var responses = document.RootElement
                    .GetProperty("responses")
                    .EnumerateArray()
                    .Select((value, i) => (value, i));

                foreach (var response in responses)
                {
                    var query = queryList[response.i];
                    yield return searchResponseParser.Parse(response.value.GetRawText(), query);
                }
            }
        }

        private PostData PrepareBulkUpdateQuery<TDocument>(string indexName, IEnumerable<TDocument> documents)
        {
            var ops = new List<object>();

            foreach (var document in documents)
            {
                var id = documentIdProvider.GetDocumentId(document);
                var serialized = pocoManager.Serialize(document);

                ops.Add(new 
                { 
                    index = new { _index = indexName, _id = id }
                });

                ops.Add(serialized);
            }

            return PostData.MultiJson(ops);
        }

        private string GetIndexName<TDocument>()
        {
            return GetIndexName(typeof(TDocument));
        }

        private string GetIndexName(Type documentType)
        {
            var attribute = searchIndexProvider.GetSearchIndex(documentType);
            return attribute.Name;
        }
    }
}