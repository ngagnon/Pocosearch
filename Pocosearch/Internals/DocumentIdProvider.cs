using System;
using System.Collections.Generic;
using System.Reflection;
using Elasticsearch.Net;

namespace Pocosearch.Internals
{
    /* @TODO: thread safety */
    public class DocumentIdProvider
    {
        private readonly IElasticLowLevelClient elasticClient;
        private readonly Dictionary<Type, PropertyInfo> cache;

        public DocumentIdProvider(IElasticLowLevelClient elasticClient)
        {
            this.elasticClient = elasticClient;
            cache = new Dictionary<Type, PropertyInfo>();
        }

        public string GetDocumentId<T>(T document)
        {
            if (!cache.TryGetValue(typeof(T), out var property))
            {
                property = FindDocumentIdProperty<T>();
                cache[typeof(T)] = property;
            }

            return property.GetValue(document).ToString();
        }

        private static PropertyInfo FindDocumentIdProperty<T>()
        {
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<DocumentIdAttribute>();

                if (attribute != null)
                {
                    if (!IsValidDocumentId(property.PropertyType))
                        throw new InvalidOperationException($"Type ${property.PropertyType.FullName} is not a valid document ID");

                    return property;
                }
            }

            throw new InvalidOperationException($"Document type {typeof(T).FullName} is missing a [DocumentId] attribute");
        }

        private static bool IsValidDocumentId(Type propertyType)
        {
            return propertyType == typeof(int)
                || propertyType == typeof(long)
                || propertyType == typeof(string)
                || propertyType == typeof(Guid);
        }
    }
}