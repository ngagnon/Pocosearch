using System;
using System.Linq;
using System.Threading;
using Pocosearch.Tests;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class SearchAsYouTypeFacts : TestBase
    {
        public SearchAsYouTypeFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<Article>();
        }

        [Fact]
        public void PartialMatches()
        {
            var article = new Article
            {
                Id = 1,
                Title = "Sleeping Octopuses May Have Dreams",
                TitleSearchAsYouType = "Sleeping Octopuses May Have Dreams",
            };

            pocosearch.SetupIndex<Article>();
            pocosearch.AddOrUpdate(article);
            pocosearch.Refresh<Article>();

            var query1 = new SearchQuery("octop");
            query1.AddSource<Article>().Exclude(x => x.TitleSearchAsYouType);

            var results1 = pocosearch.Search(query1);
            var articles1 = results1.GetDocumentsOfType<Article>();
            articles1.Count().ShouldBe(0);

            var query2 = new SearchQuery("octop");
            query2.AddSource<Article>().Exclude(x => x.Title);

            var results2 = pocosearch.Search(query2);
            var articles2 = results2.GetDocumentsOfType<Article>();
            articles2.Count().ShouldBe(1);
        }

        [SearchIndex("article_search_as_you_type_facts")]
        public class Article
        {
            [DocumentId]
            public int Id { get; set; }

            [FullText]
            public string Title { get; set; }

            [FullText(SearchAsYouType = true)]
            public string TitleSearchAsYouType { get; set; }
        }
    }
}