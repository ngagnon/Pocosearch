using System;
using System.Linq;
using System.Threading;
using Pocosearch.Tests;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class FilterFacts : TestBase
    {
        public FilterFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<Hero>();
        }

        [Fact]
        public void SimpleFilter()
        {
            pocosearch.SetupIndex<Hero>();

            var toAdd = Enumerable.Range(1, 5).Select(i => new Hero
            {
                Id = i,
                Name = "Barbarian",
                Description = "Brutal warrior",
                Health = i * 100,
                Mana = 0,
                LastLoginDate = DateTime.Now.AddMonths(-1 * i)
            });

            pocosearch.BulkAddOrUpdate(toAdd);
            pocosearch.Refresh<Hero>();

            var query = new SearchQuery("brutal");
            var source = query.AddSource<Hero>();
            source.Filter(x => x.Health >= 300);

            var results = pocosearch.Search(query);
            var cars = results.GetDocumentsOfType<Hero>().ToList();
            cars.Count.ShouldBe(3);
        }

        [Fact]
        public void ComplexFilter()
        {
            pocosearch.SetupIndex<Hero>();

            var now = new DateTime(2020, 03, 13);
            var toAdd = Enumerable.Range(1, 5).Select(i => new Hero
            {
                Id = i,
                Name = "Barbarian",
                Description = "Brutal warrior",
                Health = i * 100,
                Mana = 0,
                LastLoginDate = now.AddMonths(-1 * i)
            });

            pocosearch.BulkAddOrUpdate(toAdd);
            pocosearch.Refresh<Hero>();

            var query = new SearchQuery("brutal");
            var source = query.AddSource<Hero>();
            source.Filter(x => x.Health == 500 || (x.Mana == 0 && x.LastLoginDate >= now.AddMonths(-2)));

            var results = pocosearch.Search(query);
            var cars = results.GetDocumentsOfType<Hero>().ToList();
            cars.Count.ShouldBe(3);
        }

        [SearchIndex("hero_filter_facts")]
        public class Hero
        {
            [DocumentId]
            public int Id { get; set; }

            [FullText]
            public string Name { get; set; }

            [FullText]
            public string Description { get; set; }

            public int Health { get; set; }
            public int Mana { get; set; }
            public DateTime LastLoginDate { get; set; }
        }
    }
}