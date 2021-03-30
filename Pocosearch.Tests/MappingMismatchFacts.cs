using System;
using System.Linq;
using System.Threading;
using Pocosearch.Tests;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class MappingMismatchFacts : TestBase
    {
        public MappingMismatchFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<Initial>();
        }

        [Fact]
        public void DetectMismatchMappings()
        {
            pocosearch.SetupIndex<Initial>();
            pocosearch.AddOrUpdate(new Initial { Id = 1, Description = "Hello, world." });
            pocosearch.Refresh<Initial>();

            var pocosearch2 = NewClient();
            Should.Throw<Exception>(() => pocosearch2.SetupIndex<Updated>());
        }

        [SearchIndex("mapping_mismatch_facts")]
        public class Initial
        {
            [DocumentId]
            public int Id { get; set; }

            [FullText]
            public string Description { get; set; }
        }

        [SearchIndex("mapping_mismatch_facts")]
        public class Updated
        {
            [DocumentId]
            public int Id { get; set; }

            public float Description { get; set; }
        }
    }
}