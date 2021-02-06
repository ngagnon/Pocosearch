using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Elasticsearch.Net;
using Pocosearch.Internals;
using Pocosearch.Utils;

namespace Pocosearch
{
    /* @TODO: check for success with every response (see error handling section) */
    /* @TODO: search-as-you-type */
    /* @TODO: async methods */
    /* @TODO: filter sources (e.g. WHERE author = 'bob') */
    /* @TODO: take Boost, Exclude into account */
    public class PocosearchClient : IPocosearchClient
    {
        private readonly IElasticLowLevelClient elasticClient;
        private readonly DocumentIdProvider documentIdProvider;
        private readonly SearchIndexConfigurationProvider searchIndexProvider;
        private readonly IndexManager indexManager;
        private readonly FullTextFieldProvider fullTextFieldProvider;

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
            documentIdProvider = new DocumentIdProvider(elasticClient);
            searchIndexProvider = new SearchIndexConfigurationProvider();
            indexManager = new IndexManager(elasticClient);
            fullTextFieldProvider = new FullTextFieldProvider();
        }

        /* @TODO: handle case where there are changes to mappings */
        public void SetupIndex<TDocument>()
        {
            var indexName = GetIndexName<TDocument>();
            indexManager.SetupIndex<TDocument>(indexName);
        }

        public void AddOrUpdate<TDocument>(TDocument document)
        {
            var indexName = GetIndexName<TDocument>();
            var id = documentIdProvider.GetDocumentId(document);

            elasticClient.Index<StringResponse>(indexName, id, PostData.Serializable(document));
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

            elasticClient.Bulk<StringResponse>(indexName, PostData.MultiJson(ops));
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
            elasticClient.Delete<StringResponse>(indexName, documentId);
        }

        public IEnumerable<SearchResult> Search(SearchQuery query)
        {
            /* @TODO: maybe change API to use AddSource<T>() and avoid this GetGenericArguments? */
            var documentTypes = query.Sources
                .Select(x => x.GetType().GetGenericArguments().First());

            var indexNameMappings = documentTypes
                .ToDictionary(x => GetIndexName(x));
            
            var subQueries = documentTypes
                .Select(type => new 
                {
                    @bool = new 
                    {
                        must = new List<object> 
                        { 
                            GetSearchQuery(query.SearchString, query.Fuzziness, type) 
                        },
                        filter = new 
                        {
                            term = new 
                            {
                                _index = GetIndexName(type)
                            }
                        }
                    }
                });

            var searchResponse = elasticClient.Search<StringResponse>(PostData.Serializable(new
            {
                from = 0,
                size = query.Limit,
                query = new
                {
                    @bool = new
                    {
                        should = subQueries
                    }
                }
            }));

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

        private object GetSearchQuery(string searchString, int fuzziness, Type documentType)
        {
            var fields = fullTextFieldProvider.GetFullTextFields(documentType);
            object multi_match;

            if (fuzziness == Fuzziness.Auto)
            {
                multi_match = new 
                {
                    query = searchString,
                    fuzziness = "AUTO",
                    fields
                };
            }
            else
            {
                multi_match = new 
                {
                    query = searchString,
                    fuzziness,
                    fields
                };
            }

            return new { multi_match };
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