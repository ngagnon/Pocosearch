using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Elasticsearch.Net;
using Pocosearch.Internals;
using Pocosearch.Utils;

namespace Pocosearch
{
    public class PocosearchClient : IPocosearchClient
    {
        private readonly IElasticLowLevelClient elasticClient;
        private readonly DocumentIdProvider documentIdProvider;
        private readonly SearchIndexConfigurationProvider searchIndexProvider;
        private readonly SearchResponseParser searchResponseParser;
        private readonly SearchQueryBuilder searchQueryBuilder;
        private readonly IndexManager indexManager;

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
            searchQueryBuilder = new SearchQueryBuilder(searchIndexProvider);
            searchResponseParser = new SearchResponseParser(searchIndexProvider);
            indexManager = new IndexManager(elasticClient);
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

            var response = elasticClient.Index<StringResponse>(indexName, id, PostData.Serializable(document));

            if (!response.Success)
                throw new ApiException(response);
        }

        public void BulkAddOrUpdate<TDocument>(IEnumerable<TDocument> documents)
        {
            var indexName = GetIndexName<TDocument>();
            var ops = new List<object>();

            foreach (var document in documents)
            {
                var id = documentIdProvider.GetDocumentId(document);

                ops.Add(new 
                { 
                    index = new { _index = indexName, _id = id }
                });

                ops.Add(document);
            }

            var response = elasticClient.Bulk<StringResponse>(indexName, PostData.MultiJson(ops));

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
            var elasticQueries = queryList.Select(q => searchQueryBuilder.Build(q)); 
            var queryPackets = new List<object>();

            foreach (var query in elasticQueries)
            {
                queryPackets.Add(new {});
                queryPackets.Add(query);
            }

            var searchResponse = elasticClient.MultiSearch<StringResponse>(
                PostData.MultiJson(queryPackets));

            if (!searchResponse.Success)
                throw new ApiException(searchResponse);


            using (var document = JsonDocument.Parse(searchResponse.Body))
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