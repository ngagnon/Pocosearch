using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pocosearch.Tests;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class MultiSearchFacts : TestBase
    {
        public MultiSearchFacts(ElasticsearchClusterFixture fixture) : base(fixture)
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

            var carQuery = new SearchQuery("strudel");
            carQuery.AddSource<Car>();

            var dogQuery = new SearchQuery("fido");
            dogQuery.AddSource<Dog>();

            var queries = new List<SearchQuery> { carQuery, dogQuery };
            var results = pocosearch.MultiSearch(queries).ToList();
            var cars = results[0].GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(1);
            cars.First().Document.ShouldBe(car);

            var dogs = results[1].GetDocumentsOfType<Dog>().ToList();
            dogs.Count.ShouldBe(1);
            dogs.First().Document.ShouldBe(dog);
        }

        [SearchIndex("car_multi_search_facts")]
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

        [SearchIndex("dog_multi_search_facts")]
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