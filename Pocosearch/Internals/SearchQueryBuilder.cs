using System;
using System.Collections.Generic;
using System.Linq;

namespace Pocosearch.Internals
{
    /// <summary>
    /// Translates a Pocosearch SearchQuery into the Elasticsearch query DSL
    /// </summary>
    public class SearchQueryBuilder
    {
        private readonly FullTextFieldProvider fullTextFieldProvider;
        private readonly SearchIndexConfigurationProvider searchIndexProvider;

        public SearchQueryBuilder(SearchIndexConfigurationProvider searchIndexProvider)
        {
            this.searchIndexProvider = searchIndexProvider;
            fullTextFieldProvider = new FullTextFieldProvider();
        }

        public object Build(SearchQuery query)
        {
            var subQueries = query.Sources
                .Select(source => new 
                {
                    @bool = new 
                    {
                        must = GetSourceQuery(query, source),
                        filter = GetSourceFilter(source)
                    }
                });

            return new
            {
                from = 0,
                size = query.Limit,
                query = new
                {
                    @bool = new
                    {
                        should = subQueries
                    }
                }
            };
        }

        private object GetSourceFilter(Source source)
        {
            var indexFilter = new 
            {
                term = new 
                {
                    _index = GetIndexName(source.DocumentType)
                }
            };

            if (source.DocumentFilter == null)
            {
                return indexFilter;
            }
            else
            {
                return new object[]
                {
                    indexFilter,
                    ConvertFilterToQuery(source.DocumentFilter)
                };
            }
        }

        private object ConvertFilterToQuery(Filter filter)
        {
            if (filter is ComparisonFilter)
            {
                return ConvertComparisonFilterToQuery((ComparisonFilter)filter);
            }
            else if (filter is FilterCombination)
            {
                var combination = (FilterCombination)filter;
                var key = combination.CombinationType == CombinationType.MatchAll
                    ? "must" 
                    : "should";

                var subQueries = combination.Filters
                    .Select(ConvertFilterToQuery)
                    .ToArray();

                return new
                {
                    @bool = new Dictionary<string, object>
                    {
                        [key] = subQueries 
                    }
                };
            }
            else
            {
                throw new ArgumentException("Unexpected filter type", nameof(filter));
            }
        }

        private object ConvertComparisonFilterToQuery(ComparisonFilter filter)
        {
            switch (filter.ComparisonType)
            {
                case ComparisonType.LessThan:
                case ComparisonType.LessThanOrEqual:
                case ComparisonType.GreaterThan:
                case ComparisonType.GreaterThanOrEqual:
                    return new
                    {
                        range = new Dictionary<string, object>
                        {
                            [filter.FieldName] = new Dictionary<string, object>
                            {
                                [GetRangeKey(filter.ComparisonType)] = filter.Value
                            }
                        }
                    };

                case ComparisonType.Equal:
                    return new
                    {
                        term = new Dictionary<string, object>
                        {
                            [filter.FieldName] = new
                            {
                                value = filter.Value
                            }
                        }
                    };

                case ComparisonType.NotEqual:
                    return new
                    {
                        @bool = new
                        {
                            must_not = new
                            {
                                term = new Dictionary<string, object>
                                {
                                    [filter.FieldName] = new
                                    {
                                        value = filter.Value
                                    }
                                }
                            }
                        }
                    };

                default:
                    throw new ArgumentException($"Unexpected comparison type '{filter.ComparisonType}'", nameof(filter));
            }
        }

        private string GetRangeKey(ComparisonType comparisonType)
        {
            return comparisonType switch
            {
                ComparisonType.LessThan => "lt",
                ComparisonType.LessThanOrEqual => "lte",
                ComparisonType.GreaterThan => "gt",
                ComparisonType.GreaterThanOrEqual => "gte",
                _ => throw new ArgumentException($"Unexpected comparison type '{comparisonType}'", nameof(comparisonType))
            };
        }

        private object GetSourceQuery(SearchQuery query, Source source)
        {
            var excludedFields = source.Fields
                .Where(x => x.Exclude)
                .Select(x => x.Name)
                .ToList();

            var fields = fullTextFieldProvider
                .GetFullTextFields(source.DocumentType)
                .Where(x => !excludedFields.Contains(x.Name));

            var queries = fields
                .Select(x => GetFieldQuery(query, source, x))
                .ToList();

            return new
            {
                dis_max = new
                {
                    queries,
                    tie_breaker = 0.3
                }
            };
        }

        private object GetFieldQuery(SearchQuery query, Source source, FullTextAttribute field)
        {
            var boost = source.Fields.Find(x => x.Name == field.Name)?.Boost ?? 1;
            var fuzziness = query.Fuzziness == Fuzziness.Auto ? "AUTO" : (object)query.Fuzziness;

            if (!field.SearchAsYouType)
            {
                return new
                {
                    match = new Dictionary<string, object>
                    {
                        [field.Name] = new
                        {
                            query = query.SearchString,
                            fuzziness,
                            boost
                        }
                    }
                };
            }
            else
            {
                var subFields = new List<string> 
                { 
                    field.Name, 
                    $"{field.Name}._2gram", 
                    $"{field.Name}._3gram" 
                };

                return new
                {
                    multi_match = new
                    {
                        query = query.SearchString,
                        type = "bool_prefix",
                        fields = subFields,
                        fuzziness,
                        boost
                    }
                };
            }
        }

        private string GetIndexName(Type documentType)
        {
            var attribute = searchIndexProvider.GetSearchIndex(documentType);
            return attribute.Name;
        }
    }
}