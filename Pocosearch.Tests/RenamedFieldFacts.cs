using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Elasticsearch.Net;
using Pocosearch.Tests;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class RenamedFieldFacts : TestBase
    {
        public RenamedFieldFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<Car>();
        }

        [Fact]
        public void TestRemappedFields()
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

            cars.Count.ShouldBe(1);
            cars.First().Document.ShouldBe(car);

            var elasticClient = GetElasticClient();
            var response = elasticClient.Indices.GetMapping<StringResponse>("car_renamed_field_facts");

            using (var document = JsonDocument.Parse(response.Body))
            {
                var mappings = document.RootElement
                    .GetProperty("car_renamed_field_facts")
                    .GetProperty("mappings")
                    .GetProperty("properties");

                mappings.TryGetProperty("brand", out _).ShouldBeTrue();
                mappings.TryGetProperty("Make", out _).ShouldBeFalse();
                mappings.TryGetProperty("model_year", out _).ShouldBeTrue();
                mappings.TryGetProperty("Year", out _).ShouldBeFalse();
            }
        }

        [SearchIndex("car_renamed_field_facts")]
        public class Car
        {
            [DocumentId]
            public int Id { get; set; }

            [FullText(Name = "brand")]
            public string Make { get; set; }

            [FullText]
            public string Model { get; set; }

            [Value(Name = "model_year")]
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