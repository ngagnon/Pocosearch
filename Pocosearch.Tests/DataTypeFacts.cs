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
    public class DataTypeFacts : TestBase
    {
        public DataTypeFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<IsValid>();
            pocosearch.DeleteIndex<HasArray>();
            pocosearch.DeleteIndex<HasCollection>();
            pocosearch.DeleteIndex<HasObject>();
        }

        [Fact]
        public void ValidDataTypes()
        {
            var obj = new IsValid
            {
                Id = 1,
                Year = 2020,
                Description = "Hello, world.",
                Identifier = Guid.Parse("b6614762-0e11-4952-94e9-070e65598b79"),
                PublishedOn = new DateTime(2020, 01, 01),
                Velocity = 100.5f,
                Acceleration = 200.2,
                IsEnabled = false
            };

            pocosearch.SetupIndex<IsValid>();
            pocosearch.AddOrUpdate(obj);
            pocosearch.Refresh<IsValid>();

            var query = new SearchQuery("hello");
            query.AddSource<IsValid>();

            var results = pocosearch.Search(query);
            results.GetDocumentsOfType<IsValid>().Count().ShouldBe(1);
            results.GetDocumentsOfType<IsValid>().First().Document.ShouldBe(obj);
        }

        [Fact]
        public void InvalidDataTypes()
        {
            Should.Throw<Exception>(() => pocosearch.SetupIndex<HasArray>());
            Should.Throw<Exception>(() => pocosearch.SetupIndex<HasCollection>());
            Should.Throw<Exception>(() => pocosearch.SetupIndex<HasObject>());
        }

        [SearchIndex("valid_data_type_facts")]
        public class IsValid
        {
            [DocumentId]
            public int Id { get; set; }

            [FullText]
            public string Description { get; set; }

            public int Year { get; set; }
            public Guid Identifier { get; set; }
            public DateTime PublishedOn { get; set; }
            public float Velocity { get; set; }
            public double Acceleration { get; set; }
            public bool IsEnabled { get; set; }

            public override bool Equals(object obj)
            {
                return obj is IsValid valid &&
                    Id == valid.Id &&
                    Description == valid.Description &&
                    Year == valid.Year &&
                    Identifier == valid.Identifier &&
                    PublishedOn == valid.PublishedOn &&
                    Velocity == valid.Velocity &&
                    Acceleration == valid.Acceleration &&
                    IsEnabled == valid.IsEnabled;
            }
        }

        [SearchIndex("array_data_type_facts")]
        public class HasArray
        {
            [DocumentId]
            public int Id { get; set; }

            public string[] Words { get; set; }
        }

        [SearchIndex("collection_data_type_facts")]
        public class HasCollection
        {
            [DocumentId]
            public int Id { get; set; }

            public List<string> Words { get; set; }
        }

        [SearchIndex("object_data_type_facts")]
        public class HasObject
        {
            [DocumentId]
            public int Id { get; set; }

            public PocosearchClient Client { get; set; }
        }
    }
}