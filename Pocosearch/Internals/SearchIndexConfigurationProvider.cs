using System;
using System.Collections.Generic;
using Pocosearch.Utils;

namespace Pocosearch.Internals
{
    /* @TODO: thread safety */
    public class SearchIndexConfigurationProvider
    {
        private readonly Dictionary<Type, SearchIndexAttribute> cache = new Dictionary<Type, SearchIndexAttribute>();

        public SearchIndexAttribute GetSearchIndex(Type documentType)
        {
            if (!cache.TryGetValue(documentType, out var attribute))
            {
                attribute = FindSearchIndex(documentType);
                cache[documentType] = attribute;
            }

            return attribute;
        }

        private static SearchIndexAttribute FindSearchIndex(Type documentType)
        {
            var attribute = documentType.GetCustomAttribute<SearchIndexAttribute>();

            if (attribute == null)
            {
                attribute = new SearchIndexAttribute(documentType.Name.ToLower());
            }

            return attribute;
        }
    }
}