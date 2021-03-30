using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Pocosearch.Utils;

namespace Pocosearch.Internals
{
    internal class SearchResponseParser
    {
        private readonly SearchIndexConfigurationProvider searchIndexProvider;
        private readonly PocoManager pocoManager;

        public SearchResponseParser(SearchIndexConfigurationProvider searchIndexProvider, PocoManager pocoManager)
        {
            this.searchIndexProvider = searchIndexProvider;
            this.pocoManager = pocoManager;
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
                        Document = pocoManager.Deserialize(hit.GetProperty("_source"), documentType)
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