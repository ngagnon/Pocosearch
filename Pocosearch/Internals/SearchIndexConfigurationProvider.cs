using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Pocosearch.Utils;

namespace Pocosearch.Internals
{
    public class SearchIndexConfigurationProvider
    {
        private readonly ConcurrentDictionary<Type, SearchIndexAttribute> cache 
            = new ConcurrentDictionary<Type, SearchIndexAttribute>();

        public SearchIndexAttribute GetSearchIndex(Type documentType)
        {
            return cache.GetOrAdd(documentType, key => FindSearchIndex(key));
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