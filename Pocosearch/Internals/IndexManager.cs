using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Pocosearch.Utils;

namespace Pocosearch.Internals
{
    public class IndexManager
    {
        private readonly IElasticLowLevelClient elasticClient;
        private readonly PocoManager pocoManager;
        private readonly Dictionary<string, bool> indexes;

        public IndexManager(IElasticLowLevelClient elasticClient, PocoManager pocoManager)
        {
            this.elasticClient = elasticClient;
            this.pocoManager = pocoManager;
            indexes = new Dictionary<string, bool>();
        }

        public void SetupIndex<TDocument>(string indexName)
        {
            if (!indexes.ContainsKey(indexName))
            {
                if (!IndexExists(indexName))
                {
                    CreateIndex<TDocument>(indexName);
                }
                else
                {
                    ValidateMappings<TDocument>(indexName);
                }

                indexes[indexName] = true;
            }
        }

        public async Task SetupIndexAsync<TDocument>(string indexName)
        {
            if (!indexes.ContainsKey(indexName))
            {
                if (!IndexExists(indexName))
                {
                    await CreateIndexAsync<TDocument>(indexName)
                        .ConfigureAwait(false);
                }
                else
                {
                    await ValidateMappingsAsync<TDocument>(indexName)
                        .ConfigureAwait(false);
                }

                indexes[indexName] = true;
            }
        }


        private bool IndexExists(string indexName)
        {
            var response = elasticClient.Indices.Exists<StringResponse>(indexName);

            if (!response.SuccessOrKnownError)
                throw new ApiException(response);

            return response.Success;
        }

        private void ValidateMappings<TDocument>(string indexName)
        {
            var response = elasticClient.Indices.GetMapping<StringResponse>(indexName);

            if (!response.Success)
                throw new ApiException(response);

            CompareMappings<TDocument>(response.Body, indexName);
        }

        private async Task ValidateMappingsAsync<TDocument>(string indexName)
        {
            var response = await elasticClient.Indices
                .GetMappingAsync<StringResponse>(indexName)
                .ConfigureAwait(false);

            if (!response.Success)
                throw new ApiException(response);

            CompareMappings<TDocument>(response.Body, indexName);
        }

        private void CompareMappings<TDocument>(string responseJson, string indexName)
        {
            using (var document = JsonDocument.Parse(responseJson))
            {
                var mappings = document.RootElement
                    .GetProperty(indexName)
                    .GetProperty("mappings")
                    .GetProperty("properties")
                    .GetObject<Dictionary<string, Mapping>>();

                var desiredMappings = GenerateMappings(typeof(TDocument));

                var mappingsDiffer = desiredMappings.Keys.Count != mappings.Keys.Count 
                    || desiredMappings.Any(x => !mappings.ContainsKey(x.Key) || mappings[x.Key].type != x.Value.type);

                if (mappingsDiffer)
                    throw new MismatchedSchemaException(indexName);
            }
        }

        private void CreateIndex<TDocument>(string name)
        {
            var properties = GenerateMappings(typeof(TDocument));
            var index = new 
            {
                mappings = new { properties }
            };

            var response = elasticClient.Indices.Create<StringResponse>(name, PostData.Serializable(index));

            if (!response.Success)
                throw new ApiException(response);
        }

        private async Task CreateIndexAsync<TDocument>(string name)
        {
            var properties = GenerateMappings(typeof(TDocument));
            var index = new 
            {
                mappings = new { properties }
            };

            var response = await elasticClient.Indices
                .CreateAsync<StringResponse>(name, PostData.Serializable(index))
                .ConfigureAwait(false);

            if (!response.Success)
                throw new ApiException(response);
        }

        private Dictionary<string, Mapping> GenerateMappings(Type documentType)
        {
            var properties = new Dictionary<string, Mapping>();
            var pocoProperties = pocoManager.GetPocoProperties(documentType);

            foreach (var property in pocoProperties)
            {
                var type = GetFieldType(property);
                properties[property.FieldName] = new Mapping { type = type };
            }

            return properties;
        }

        private static string GetFieldType(PocoProperty property)
        {
            if (property.Type == typeof(string))
            {
                return property switch
                {
                    { IsFullText: false } => "keyword",
                    { SearchAsYouType: true } => "search_as_you_type",
                    { SearchAsYouType: false } => "text"
                };
            }

            return property.Type.FullName switch
            {
                "System.Int32" => "integer",
                "System.Int64" => "long",
                "System.Guid" => "keyword",
                "System.DateTime" => "date",
                _ => throw new ArgumentException("Unsupported field type", nameof(propertyInfo))
            };
        }

        private class Mapping
        {
            public string type { get; set; }
        }
    }
}