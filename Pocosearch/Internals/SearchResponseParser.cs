using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Pocosearch.Utils;

namespace Pocosearch.Internals
{
    public class SearchResponseParser
    {
        private readonly SearchIndexConfigurationProvider searchIndexProvider;

        public SearchResponseParser(SearchIndexConfigurationProvider searchIndexProvider)
        {
            this.searchIndexProvider = searchIndexProvider;
        }

        public SearchResultCollection Parse(string responseJson, SearchQuery query)
        {
            return new SearchResultCollection(DoParse(responseJson, query));
        }

        private IEnumerable<SearchResult> DoParse(string responseJson, SearchQuery query)
        {
            var documentTypes = query.Sources.Select(x => x.DocumentType);
            var indexNameMappings = documentTypes
                .ToDictionary(x => GetIndexName(x));
            
            using (var document = JsonDocument.Parse(responseJson))
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

        private string GetIndexName(Type documentType)
        {
            var attribute = searchIndexProvider.GetSearchIndex(documentType);
            return attribute.Name;
        }
    }
}