using System;
using System.Linq;
using System.Threading;
using Pocosearch.Tests;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class MultiIndexFacts : TestBase
    {
        public MultiIndexFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<Car>();
            pocosearch.DeleteIndex<Dog>();
        }

        [Fact]
        public void SearchFromMultipleSources()
        {
            pocosearch.SetupIndex<Car>();
            pocosearch.SetupIndex<Dog>();

            var car = new Car
            {
                Id = 1,
                Make = "German Car",
                Model = "Strudel",
                Year = 2020
            };

            pocosearch.AddOrUpdate(car);
            pocosearch.Refresh<Car>();

            var dog = new Dog
            {
                Id = 1,
                Name = "Fido",
                Breed = "German Shepard",
                BirthDate = new DateTime(2016, 01, 22)
            };

            pocosearch.AddOrUpdate(dog);
            pocosearch.Refresh<Dog>();

            var query = new SearchQuery("german");
            query.AddSource<Car>();
            query.AddSource<Dog>();

            var results = pocosearch.Search(query);
            var cars = results.GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(1);
            cars.First().Document.ShouldBe(car);

            var dogs = results.GetDocumentsOfType<Dog>().ToList();
            dogs.Count.ShouldBe(1);
            dogs.First().Document.ShouldBe(dog);

            query.SearchString = "shepard";
            results = pocosearch.Search(query);
            cars = results.GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(0);
            dogs = results.GetDocumentsOfType<Dog>().ToList();
            dogs.Count.ShouldBe(1);
            dogs.First().Document.ShouldBe(dog);
        }

        [SearchIndex("car_multi_index_facts")]
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

        [SearchIndex("dog_multi_index_facts")]
        public class Dog
        {
            [DocumentId]
            public int Id { get; set; }

            [FullText]
            public string Name { get; set; }

            [FullText]
            public string Breed { get; set; }

            public DateTime BirthDate { get; set; }

            public override bool Equals(object obj)
            {
                return obj is Dog dog &&
                       Id == dog.Id &&
                       Name == dog.Name &&
                       Breed == dog.Breed &&
                       BirthDate == dog.BirthDate;
            }
        }
    }
}