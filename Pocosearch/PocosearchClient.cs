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
    /* @TODO: multi-query */
    /* @TODO: SetupIndex should automatically add new fields, or crash with an exception
       if a field was renamed or changed type, etc. */
    public class PocosearchClient : IPocosearchClient
    {
        private readonly IElasticLowLevelClient elasticClient;
        private readonly DocumentIdProvider documentIdProvider;
        private readonly SearchQueryBuilder searchQueryBuilder;
        private readonly SearchIndexConfigurationProvider searchIndexProvider;
        private readonly IndexManager indexManager;

        public PocosearchClient() : this(new ElasticLowLevelClient())
        {
        }

        public PocosearchClient(Pocosearch.ConnectionConfiguration settings)
            : this(new ElasticLowLevelClient(settings))
        {
        }

        private PocosearchClient(IElasticLowLevelClient elasticClient)
        {
            this.elasticClient = elasticClient;
            documentIdProvider = new DocumentIdProvider();
            searchIndexProvider = new SearchIndexConfigurationProvider();
            searchQueryBuilder = new SearchQueryBuilder(searchIndexProvider);
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
            var documentTypes = query.Sources.Select(x => x.DocumentType);
            var indexNameMappings = documentTypes
                .ToDictionary(x => GetIndexName(x));
            
            var elasticQuery = searchQueryBuilder.Build(query); 

            var searchResponse = elasticClient.Search<StringResponse>(
                PostData.Serializable(elasticQuery));

            if (!searchResponse.Success)
                throw new ApiException(searchResponse);

            var body = searchResponse.Body;

            using (var document = JsonDocument.Parse(body))
            {
                var hits = document.RootElement
                    .GetProperty("hits")
                    .GetProperty("hits")
                    .EnumerateArray();

                foreach (var hit in hits)
                {
                    var indexName = hit.GetProperty("_index").GetString();
                    var documentType = indexNameMappings[indexName];
                    var score = hit.GetProperty("_score").GetDouble();

                    yield return new SearchResult
                    {
                        DocumentType = documentType,
                        Score = score,
                        Document = hit.GetProperty("_source").GetObject(documentType)
                    };
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