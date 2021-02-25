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
    /* @TODO: filter sources (e.g. WHERE author = 'bob') */
    /* @TODO: SetupIndex should automatically add new fields, or crash with an exception
       if a field was renamed or changed type, etc. */
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
            documentIdProvider = new DocumentIdlasticClient();
            searchIndexProvider = new SearchIndexConfigurationProvider();
            indexManager = new IndexManager(elasticClient);
            fullTextFieldProvider = new FullTextFieldProvider();
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
                throw new PocosearchException(response);
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
                throw new PocosearchException(response);
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
                throw new PocosearchException(response);
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
                            GetSourceQuery(query, source) 
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

            var fullQuery = new
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
            };

            var searchResponse = elasticClient.Search<StringResponse>(
                PostData.Serializable(fullQuery));

            if (!searchResponse.Success)
                throw new PocosearchException(searchResponse);

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

        private object GetSourceQuery(SearchQuery query, Source source)
        {
            var excludedFields = source.Fields
                .Where(x => x.Exclude)
                .Select(x => x.Name)
                .ToList();

            var fields = fullTextFieldProvider
                .GetFullTextFields(source.DocumentType)
                .Where(x => !excludedFields.Contains(x.Name));

            var queries = fields
                .Select(x => GetFieldQuery(query, source, x))
                .ToList();

            return new
            {
                dis_max = new
                {
                    queries,
                    tie_breaker = 0.3
                }
            };
        }

        private object GetFieldQuery(SearchQuery query, Source source, FullTextAttribute field)
        {
            var boost = source.Fields.Find(x => x.Name == field.Name)?.Boost ?? 1;
            var fuzziness = query.Fuzziness == Fuzziness.Auto ? "AUTO" : (object)query.Fuzziness;

            if (!field.SearchAsYouType)
            {
                return new
                {
                    match = new Dictionary<string, object>
                    {
                        { 
                            field.Name,
                            new Dictionary<string, object>
                            {
                                { "query", query.SearchString },
                                { "fuzziness", fuzziness },
                                { "boost", boost }
                            }
                        }
                    }
                };
            }
            else
            {
                var subFields = new List<string> 
                { 
                    field.Name, 
                    $"{field.Name}._2gram", 
                    $"{field.Name}._3gram" 
                };

                return new
                {
                    multi_match = new Dictionary<string, object>
                    {
                        { "query", query.SearchString },
                        { "type", "bool_prefix" },
                        { "fields", subFields },
                        { "fuzziness", fuzziness },
                        { "boost", boost }
                    }
                };
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