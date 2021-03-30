using System;
using System.Linq;
using System.Threading;
using Pocosearch.Tests;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class AddOrUpdateFacts : TestBase
    {
        public AddOrUpdateFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<Car>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestAdd(bool setupFirst)
        {
            var car = new Car
            {
                Id = 1,
                Make = "Beep",
                Model = "Corona GT",
                Year = 2020
            };

            if (setupFirst)
                pocosearch.SetupIndex<Car>();

            pocosearch.AddOrUpdate(car);
            pocosearch.Refresh<Car>();

            var query = new SearchQuery("corona");
            query.AddSource<Car>();

            var results = pocosearch.Search(query);
            var cars = results.GetDocumentsOfType<Car>().ToList();

            cars.Count.ShouldBe(1);
            cars.First().Document.ShouldBe(car);
        }

        [Fact]
        public void TestUpdate()
        {
            var car1 = new Car
            {
                Id = 1,
                Make = "Beep",
                Model = "Corona GT",
                Year = 2020
            };

            pocosearch.SetupIndex<Car>();
            pocosearch.AddOrUpdate(car1);

            var car2 = new Car
            {
                Id = 1,
                Make = "Corona",
                Model = "Fart HD",
                Year = 2000
            };

            pocosearch.AddOrUpdate(car2);
            pocosearch.Refresh<Car>();

            var query = new SearchQuery("corona");
            query.AddSource<Car>();

            var results = pocosearch.Search(query);
            var cars = results.GetDocumentsOfType<Car>().ToList();

            cars.Count.ShouldBe(1);
            cars.First().Document.ShouldBe(car2);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestBulkAdd(bool setupFirst)
        {
            if (setupFirst)
                pocosearch.SetupIndex<Car>();

            var toAdd = Enumerable.Range(1, 5).Select(i => new Car
            {
                Id = i,
                Make = (i % 2 == 0) ? "Beep" : "Zoom",
                Model = $"Corona GT{i}",
                Year = 2020
            });

            pocosearch.BulkAddOrUpdate(toAdd);
            pocosearch.Refresh<Car>();

            var query = new SearchQuery("corona");
            query.AddSource<Car>();

            var results = pocosearch.Search(query);
            var cars = results.GetDocumentsOfType<Car>().ToList();

            cars.Count.ShouldBe(5);
            cars.Select(x => x.Document).ShouldBe(toAdd.ToArray());

            query.SearchString = "beep";
            results = pocosearch.Search(query);
            cars = results.GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(2);
        }

        [SearchIndex("car_add_or_update_facts")]
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