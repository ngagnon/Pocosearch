using System;
using System.Linq;
using System.Text.Json;
using Elasticsearch.Net;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class IgnoredFieldFacts : TestBase
    {
        public IgnoredFieldFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<Car>();
            pocosearch.DeleteIndex<IgnoredDocumentId>();
            pocosearch.DeleteIndex<IgnoredFullTextField>();
            pocosearch.DeleteIndex<IgnoredValueField>();
        }

        [Fact]
        public void IgnoredFieldNotPersisted()
        {   
            var car = new Car
            {
                Id = 1,
                Make = "Beep",
                Model = "Corona GT",
                Year = 2020
            };

            pocosearch.AddOrUpdate(car);
            pocosearch.Refresh<Car>();

            var query = new SearchQuery("corona");
            query.AddSource<Car>();

            var results = pocosearch.Search(query);
            var cars = results.GetDocumentsOfType<Car>().ToList();

            cars.Count.ShouldBe(1);
            cars.First().Document.ShouldNotBe(car);

            car.Year = default(int);
            cars.First().Document.ShouldBe(car);
        }

        [Fact]
        public void IgnoredFieldNotInMappings()
        {
            var car = new Car
            {
                Id = 1,
                Make = "Beep",
                Model = "Corona GT",
                Year = 2020
            };

            pocosearch.AddOrUpdate(car);
            pocosearch.Refresh<Car>();

            var elasticClient = GetElasticClient();
            var response = elasticClient.Indices.GetMapping<StringResponse>("car_ignored_field_facts");

            using (var document = JsonDocument.Parse(response.Body))
            {
                var mappings = document.RootElement
                    .GetProperty("car_ignored_field_facts")
                    .GetProperty("mappings")
                    .GetProperty("properties");

                mappings.TryGetProperty("Year", out _).ShouldBeFalse();
            }
        }

        [Fact]
        public void CannotIgnoreDocumentId()
        {
            Should.Throw<Exception>(() => pocosearch.SetupIndex<IgnoredDocumentId>());
        }

        [Fact]
        public void CannotIgnoreValueField()
        {
            Should.Throw<Exception>(() => pocosearch.SetupIndex<IgnoredValueField>());
        }

        [Fact]
        public void CannotIgnoreFullTextField()
        {
            Should.Throw<Exception>(() => pocosearch.SetupIndex<IgnoredFullTextField>());
        }

        [SearchIndex("car_ignored_field_facts")]
        public class Car
        {
            [DocumentId]
            public int Id { get; set; }

            public string Make { get; set; }

            [FullText]
            public string Model { get; set; }

            [Ignore]
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

        [SearchIndex("document_id_ignored_field_facts")]
        public class IgnoredDocumentId
        {
            [DocumentId, Ignore]
            public int Id { get; set; }
        }

        [SearchIndex("value_ignored_field_facts")]
        public class IgnoredValueField
        {
            [DocumentId]
            public int Id { get; set; }

            [Value(Name = "label"), Ignore]
            public string Name { get; set; }
        }

        [SearchIndex("full_text_ignored_field_facts")]
        public class IgnoredFullTextField
        {
            [DocumentId]
            public int Id { get; set; }

            [FullText, Ignore]
            public string Name { get; set; }
        }


    }
}
