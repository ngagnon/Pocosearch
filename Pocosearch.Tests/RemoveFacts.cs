using System;
using System.Linq;
using System.Threading;
using Pocosearch.Tests;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class RemoveFacts : TestBase
    {
        public RemoveFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<Car>();
        }

        [Fact]
        public void TestRemove()
        {
            var toAdd = Enumerable.Range(1, 5).Select(i => new Car
            {
                Id = i,
                Make = (i % 2 == 0) ? "Beep" : "Zoom",
                Model = $"Corona GT{i}",
                Year = 2020
            });

            pocosearch.SetupIndex<Car>();
            pocosearch.BulkAddOrUpdate(toAdd);
            pocosearch.Refresh<Car>();

            var query = new SearchQuery("corona");
            query.AddSource<Car>();

            var results = pocosearch.Search(query);
            var cars = results.GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(5);

            pocosearch.Remove<Car>(1);
            pocosearch.Remove<Car>(2);
            pocosearch.Refresh<Car>();

            results = pocosearch.Search(query);
            cars = results.GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(3);
            cars.Select(x => x.Document).ShouldBe(toAdd.Skip(2).ToArray());
        }

        [SearchIndex("car_remove_facts")]
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