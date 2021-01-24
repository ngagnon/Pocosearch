using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FultonSearch.Core;
using FultonSearch.DAL;
using FultonSearch.Entities;

namespace FultonSearch
{
    public class FultonClient : IFultonClient
    {
        private readonly IStorageBackend backend;
        private readonly HashAlgorithm sha256 = SHA256.Create();

        public FultonClient(IDbConnection conn)
        {
            backend = new SQLStorageBackend(conn);
        }

        public void Scan<TIndex>() where TIndex : Index
        {
            try
            {
                var index = (TIndex)Activator.CreateInstance(typeof(TIndex));

                backend.ScanIndex(index.ID, index.ScanQuery, 
                    doc => AddOrUpdate<TIndex>(doc.ID, doc.Content));
            }
            catch (Exception e)
            {
                throw new FultonException("Unable to scan index", e);
            }
        }

        public void AddOrUpdate<TIndex>(int docId, string content) where TIndex : Index
        {
            try
            {
                var index = (TIndex)Activator.CreateInstance(typeof(TIndex));
                var tokens = index.Tokenize(content).ToList();
                var checksum = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));

                backend.AddOrUpdateDocument(index.ID, docId, tokens, checksum);
            }
            catch (Exception e)
            {
                throw new FultonException("Unable to add or update document", e);
            }
        }

        public bool TryRemove<TIndex>(int docId) where TIndex : Index
        {
            try
            {
                var index = (TIndex)Activator.CreateInstance(typeof(TIndex));
                return backend.TryRemoveDocument(docId, index.ID);
            }
            catch (Exception e)
            { 
                throw new FultonException("Unable to remove document", e);
            }
        }

        public IEnumerable<SearchResult> Search<TIndex>(string query) where TIndex : Index
        {
            try
            {
                return Search<TIndex>(new FullTextQuery<TIndex> { Predicate = x => x.Matches(query) });
            }
            catch (Exception e)
            { 
                throw new FultonException("Search attempt failed", e);
            }
        }

        public IEnumerable<SearchResult> Search<TIndex>(FullTextQuery<TIndex> query) where TIndex : Index
        {
            var results = GenerateSearchResults(query, typeof(TIndex));

            return query.Limit != null
                ? results.OrderByDescending(x => x.Score).Take(query.Limit.Value)
                : results;
        }

        public IEnumerable<SearchResult> Search(CompoundQuery query)
        {
            return ExecuteQuery(query);
        }

        private IEnumerable<SearchResult> ExecuteQuery(IQuery query)
        { 
            if (query is FullTextQuery)
            {
                var indexType = query.GetType().GetGenericArguments()[0];
                return GenerateSearchResults(query as FullTextQuery, indexType);
            }
            else
            {
                var compound = query as CompoundQuery;
                return ExecuteCompoundQuery(compound);
            }
        }

        private IEnumerable<SearchResult> ExecuteCompoundQuery(CompoundQuery compound)
        { 
            var allResults = compound.Queries
                .Select(q => ExecuteQuery(q).ToDictionary(r => r.DocumentId, r => r.Score))
                .ToList();
            var allDocumentIds = allResults.SelectMany(x => x.Keys).Distinct();

            foreach (var documentId in allDocumentIds)
            {
                double overallScore = 0;
                int matchedQueries = 0;
            
                foreach (var queryResults in allResults)
                { 
                    if (queryResults.TryGetValue(documentId, out var score))
                    {
                        matchedQueries++;
                        overallScore += score;
                    }
                }

                if (matchedQueries >= compound.MinimumMatch)
                {
                    yield return new SearchResult
                    {
                        DocumentId = documentId,
                        Score = overallScore
                    };
                }
            }
        }

        private IEnumerable<SearchResult> GenerateSearchResults(FullTextQuery query, Type indexType)
        {
            var index = (Index)Activator.CreateInstance(indexType);
            var predicate = query.Predicate.Compile();
            var idf = new Dictionary<string, double>();

            var tokens = PredicateParser.ExtractTokens(query.Predicate)
                .SelectMany(t => index.Tokenize(t.Value).Select(x => new SearchToken
                {
                    Value = x,
                    Boost = t.Boost
                }))
                .ToList();

            var tokenValues = tokens.Select(x => x.Value).ToList();
            var searchData = backend.GatherSearchData(index.ID, tokenValues, query.Subset);

            foreach (var token in tokenValues)
            {
                var docFreq = searchData.Results.Where(x => x.Token == token).Count();
                idf[token] = 1 + Math.Log10(searchData.NumDocs / (docFreq + 1));
            }

            foreach (var docResults in searchData.ResultsByDoc)
            {
                var candidate = GetDocumentCandidate(docResults, tokenValues);

                if (!predicate(candidate))
                    continue;

                var coord = query.EnableCoordination
                    ? docResults.Count() / tokens.Count
                    : 1;
                var norm = query.EnableFieldLengthNorm
                    ? searchData.Norms[docResults.Key]
                    : 1;
                var sumOfWeights = 0d;

                foreach (var token in tokens)
                {
                    var termFrequency = query.EnableTermFrequency
                        ? docResults.FirstOrDefault(x => x.Token == token.Value)?.Frequency ?? 0
                        : 1;

                    sumOfWeights += termFrequency
                        * Math.Pow(idf[token.Value], 2)
                        * token.Boost;
                }

                yield return new SearchResult
                {
                    DocumentId = docResults.Key,
                    Score = coord * norm * sumOfWeights
                };
            }
        }

        private static DocumentCandidate GetDocumentCandidate(IEnumerable<TermFrequency> termFrequencies, List<string> tokens)
        {
            var matchedTerms = tokens
                .Where(t => termFrequencies.Any(f => f.Token == t))
                .ToList();

            return new DocumentCandidate(matchedTerms);
        }

        public void Dispose()
        {
            backend.Dispose();
        }
    }
}
