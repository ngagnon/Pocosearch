using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Elasticsearch.Net;

namespace Pocosearch.Internals
{
    public class DocumentIdProvider
    {
        private readonly ConcurrentDictionary<Type, PropertyInfo> cache
            = new ConcurrentDictionary<Type, PropertyInfo>();

        public string GetDocumentId<T>(T document)
        {
            var property = cache.GetOrAdd(typeof(T), key => FindDocumentIdProperty(key));
            return property.GetValue(document).ToString();
        }

        private static PropertyInfo FindDocumentIdProperty(Type documentType)
        {
            var properties = documentType.GetProperties();

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

            throw new InvalidOperationException($"Document type {documentType.FullName} is missing a [DocumentId] attribute");
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