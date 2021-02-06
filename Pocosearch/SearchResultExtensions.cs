using System;
using System.Collections.Generic;
using System.Linq;

namespace Pocosearch
{
    public static class SearchResultExtensions
    {
        public static IEnumerable<SearchResult<T>> GetDocumentsOfType<T>(this IEnumerable<SearchResult> searchResults) where T : class
        {
            return searchResults
                .Where(x => x.DocumentType == typeof(T))
                .Select(x => new SearchResult<T> 
                {
                    Score = x.Score,
                    Document = x.Document as T
                });
        }
    }
}