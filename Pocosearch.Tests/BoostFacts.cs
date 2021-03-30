using System;
using System.Linq;
using System.Threading;
using Pocosearch.Tests;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class BoostFacts : TestBase
    {
        public BoostFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<Car>();
        }

        [Fact]
        public void BoostField()
        {
            pocosearch.SetupIndex<Car>();

            var car1 = new Car
            {
                Id = 1,
                Make = "Beep",
                Model = "Corona GT",
                Year = 2020
            };

            pocosearch.AddOrUpdate(car1);

            var car2 = new Car
            {
                Id = 2,
                Make = "Corana",
                Model = "Fart HD",
                Year = 2000
            };

            pocosearch.AddOrUpdate(car2);

            pocosearch.Refresh<Car>();

            var query = new SearchQuery("corona");
            var source = query.AddSource<Car>();
            source.Configure(x => x.Make, boost: 10);

            var results = pocosearch.Search(query);
            var cars = results.GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(2);
            cars[0].Document.ShouldBe(car2);

            query = new SearchQuery("corona");
            source = query.AddSource<Car>();
            source.Configure(x => x.Model, boost: 10);

            results = pocosearch.Search(query);
            cars = results.GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(2);
            cars[0].Document.ShouldBe(car1);
        }

        [SearchIndex("car_boost_facts")]
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