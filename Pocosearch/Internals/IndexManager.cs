using System;
using System.Collections.Generic;
using System.Reflection;
using Elasticsearch.Net;

namespace Pocosearch.Internals
{
    public class IndexManager
    {
        private readonly IElasticLowLevelClient elasticClient;
        private readonly Dictionary<string, bool> indexes;

        public IndexManager(IElasticLowLevelClient elasticClient)
        {
            this.elasticClient = elasticClient;
            indexes = new Dictionary<string, bool>();
        }

        public void SetupIndex<TDocument>(string indexName)
        {
            if (!indexes.ContainsKey(indexName))
            {
                if (!IndexExists(indexName))
                    CreateIndex<TDocument>(indexName);

                indexes[indexName] = true;
            }
        }

        private bool IndexExists(string indexName)
        {
            var response = elasticClient.Indices.Exists<BytesResponse>(indexName);
            return response.Success;
        }

        private void CreateIndex<TDocument>(string name)
        {
            var properties = new Dictionary<string, object>();
            var documentProperties = typeof(TDocument).GetProperties();

            foreach (var property in documentProperties)
            {
                var type = GetFieldType(property);
                properties[property.Name] = new { type };
            }

            var index = new 
            {
                mappings = new { properties }
            };

            elasticClient.Indices.Create<BytesResponse>(name, PostData.Serializable(index));
        }

        private static string GetFieldType(PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType == typeof(string))
            {
                var attribute = propertyInfo.GetCustomAttribute<FullTextAttribute>();

                return attribute switch
                {
                    null => "keyword",
                    { SearchAsYouType: true } => "search_as_you_type",
                    { SearchAsYouType: false } => "text"
                };
            }

            return propertyInfo.PropertyType.FullName switch
            {
                "System.Int32" => "integer",
                "System.Int64" => "long",
                "System.Guid" => "keyword",
                "System.DateTime" => "date",
                _ => throw new ArgumentException("Unsupported field type", nameof(propertyInfo))
            };
        }
    }
}