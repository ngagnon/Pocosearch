using System;
using System.Collections.Generic;

namespace Pocosearch
{
    public class FullTextAttribute : Attribute
    {
        public FullTextTokenizer Tokenizer { get; set; } = FullTextTokenizer.Basic;
        public string Name { get; set; }

        public FullTextAttribute(string name)
        {
            Name = name;
        }

        public FullTextAttribute()
        {
        }

        public virtual IEnumerable<string> Tokenize(string input)
        {
            switch (Tokenizer)
            {
                case FullTextTokenizer.Basic:
                    throw new NotImplementedException();
                    break;

                default:
                    throw new NotImplementedException();
                    break;
            }
        }
    }

    public enum FullTextTokenizer
    {
        Basic
    }

    public class DbConnection : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public static class DbConnectionExtensions
    {
        public static void Add<T>(this DbConnection connection, T document)
        {
            throw new NotImplementedException();
        }

        public static void Update<T>(this DbConnection connection, T document)
        {
            throw new NotImplementedException();
        }

        public static void AddOrUpdate<T>(this DbConnection connection, T document)
        {
            throw new NotImplementedException();
        }

        public static void Remove<T>(this DbConnection connection, Guid documentId)
        {

        }

        public static void Remove<T>(this DbConnection connection, int documentId)
        {

        }

        public static IEnumerable<SearchResult> Search(this DbConnection conn, SearchQuery query)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<T> GetDocumentsOfType<T>(this IEnumerable<SearchResult> searchResults)
        {
            throw new NotImplementedException();
        }
    }

    ///
    /// Good defaults for Web apps:
    ///
    /// - Limit = 100
    /// - Fuzziness = 2
    /// - Searches in all the full text fields of each index
    ///
    public class SearchQuery
    {
        public string SearchString { get; set; }
        public int Limit { get; set; } = 100;
        public int Fuzziness { get; set; } = 2;
        public List<Source> Sources { get; set; }
    }

    public abstract class Source
    {}

    public class Source<TDocument> : Source
    {
        public List<Field<TDocument>> Fields { get; set; } = null;
    }

    public class Field<TDocument>
    {
        public Func<TDocument, string> Getter { get; set; }
        public double Boost { get; set; } = 1.0;
    }

    public static class Program
    {
        public static void Main()
        {
            using (var conn = new DbConnection())
            {
                var query = new SearchQuery
                {
                    SearchString = "blue elephant",
                    Sources = new List<Source>
                    {
                        new Source<Article>()
                    }
                };

                var searchResults = conn.Search(query);
                var articles = searchResults.GetDocumentsOfType<Article>();
            }
        }
    }

    public class SearchResult
    {
        public double Rank { get; set; }
        public object Document { get; set; }
        public Type DocumentType { get; set; }
    }

    public class Article
    {
        public int Id { get; set; }

        [FullText]
        public string Title { get; set; }

        [FullText]
        public string Body { get; set; }

        public Guid Author { get; set; }
        public DateTime PublishedOn { get; set; }
    }
}