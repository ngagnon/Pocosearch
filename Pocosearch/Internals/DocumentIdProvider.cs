using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elasticsearch.Net;

namespace Pocosearch.Internals
{
    public class DocumentIdProvider
    {
        private readonly ConcurrentDictionary<Type, PropertyInfo> cache
            = new ConcurrentDictionary<Type, PropertyInfo>();

        public void ValidateDocumentId<TDocument>()
        {
            cache.GetOrAdd(typeof(TDocument), key => FindDocumentIdProperty(key));
        }

        public string GetDocumentId<T>(T document)
        {
            var property = cache.GetOrAdd(typeof(T), key => FindDocumentIdProperty(key));
            return property.GetValue(document).ToString();
        }

        private static PropertyInfo FindDocumentIdProperty(Type documentType)
        {
            var properties = documentType.GetProperties();
            var idProperties = properties
                .Where(p => p.GetCustomAttribute<DocumentIdAttribute>() != null)
                .ToList();

            if (idProperties.Count == 0)
                throw new InvalidOperationException($"Document type {documentType.FullName} is missing a [DocumentId] attribute");

            if (idProperties.Count > 1)
                throw new InvalidOperationException($"Document type {documentType.FullName} has multiple [DocumentId] attributes (there should be only 1)");

            var property = idProperties.First();

            if (!IsValidDocumentId(property.PropertyType))
                throw new InvalidOperationException($"Type ${property.PropertyType.FullName} is not a valid document ID");

            return property;
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