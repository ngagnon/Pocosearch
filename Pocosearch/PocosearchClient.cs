using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Elasticsearch.Net;
using Pocosearch.Internals;
using Pocosearch.Utils;

namespace Pocosearch
{
    /* @TODO: search-as-you-type */
    /* @TODO: filter sources (e.g. WHERE author = 'bob') */
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
            var documentTypes = query.Sources.Select(x => x.DocumentType);
            var indexNameMappings = documentTypes
                .ToDictionary(x => GetIndexName(x));
            
            var subQueries = query.Sources
                .Select(source => new 
                {
                    @bool = new 
                    {
                        must = new List<object> 
                        { 
                            GetDocumentQuery(query, source) 
                        },
                        filter = new 
                        {
                            term = new 
                            {
                                _index = GetIndexName(source.DocumentType)
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

        private object GetDocumentQuery(SearchQuery query, Source source)
        {
            var excludedFields = source.Fields.Where(x => x.Exclude).Select(x => x.Name).ToList();
            var fields = fullTextFieldProvider
                .GetFullTextFields(source.DocumentType)
                .Where(x => !excludedFields.Contains(x))
                .Select(x => ApplyBoost(x, source))
                .ToList();

            object multi_match;

            if (query.Fuzziness == Fuzziness.Auto)
            {
                multi_match = new 
                {
                    query = query.SearchString,
                    fuzziness = "AUTO",
                    fields
                };
            }
            else
            {
                multi_match = new 
                {
                    query = query.SearchString,
                    fuzziness = query.Fuzziness,
                    fields
                };
            }

            return new { multi_match };
        }

        private string ApplyBoost(string fieldName, Source source)
        {
            var boost = source.Fields.Find(x => x.Name == fieldName)?.Boost ?? 1;
            return boost > 1 ? $"{fieldName}^{boost}" : fieldName;
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