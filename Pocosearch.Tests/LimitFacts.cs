using System;
using System.Linq;
using System.Threading;
using Pocosearch.Tests;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class LimitFacts : TestBase
    {
        public LimitFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<Car>();
        }

        [Fact]
        public void TestLimit()
        {
            pocosearch.SetupIndex<Car>();

            var toAdd = Enumerable.Range(1, 10).Select(i => new Car
            {
                Id = i,
                Make = "Beep",
                Model = $"Corona GT{i}",
                Year = 2020
            });

            pocosearch.BulkAddOrUpdate(toAdd);
            pocosearch.Refresh<Car>();

            var query = new SearchQuery
            {
                SearchString = "corona",
                Limit = 5
            };

            query.AddSource<Car>();

            var results = pocosearch.Search(query);
            var cars = results.GetDocumentsOfType<Car>().ToList();

            cars.Count.ShouldBe(5);
        }

        [SearchIndex("car_limit_facts")]
        public class Car
        {
            [DocumentId]
            public int Id { get; set; }

            [FullText]
            public string Make { get; set; }

            [FullText]
            public string Model { get; set; }

            public int Year { get; set; }

            public override bool Equals(object obj)
            {
                return obj is Car car &&
                       Id == car.Id &&
                       Make == car.Make &&
                       Model == car.Model &&
                       Year == car.Year;
            }
        }
    }
}