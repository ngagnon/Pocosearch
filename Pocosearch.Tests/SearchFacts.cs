using System;
using System.Linq;
using System.Threading;
using Pocosearch.Tests;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class SearchFacts : TestBase
    {
        public SearchFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<Car>();
        }

        [Fact]
        public void CanOnlySearchFullTextFields()
        {
            var car = new Car
            {
                Id = 1,
                Make = "Beep",
                Model = "Corona GT",
                Year = 2020
            };

            pocosearch.SetupIndex<Car>();
            pocosearch.AddOrUpdate(car);
            pocosearch.Refresh<Car>();

            var query = new SearchQuery("corona");
            query.AddSource<Car>();

            var results = pocosearch.Search(query);
            var cars = results.GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(0);

            query.SearchString = "beep";
            results = pocosearch.Search(query);
            cars = results.GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(1);
            cars.First().Document.ShouldBe(car);
        }

        [Fact]
        public void CannotSearchFromNonExistingIndex()
        {
            var query = new SearchQuery("corona");
            query.AddSource<Car>();

            Should.Throw<Exception>(() => pocosearch.Search(query).ToList());
        }

        [SearchIndex("car_search_facts")]
        public class Car
        {
            [DocumentId]
            public int Id { get; set; }

            [FullText]
            public string Make { get; set; }

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