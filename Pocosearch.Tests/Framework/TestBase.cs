using System;
using Elasticsearch.Net;

namespace Pocosearch.Tests.Framework
{
    public class TestBase
    {
        protected readonly ElasticsearchClusterFixture fixture;
        protected readonly IPocosearchClient pocosearch;

        public TestBase(ElasticsearchClusterFixture fixture)
        {
            this.fixture = fixture;
            pocosearch = new PocosearchClient(fixture.ConnectionConfiguration);
        }

        protected PocosearchClient NewClient()
        {
            return new PocosearchClient(fixture.ConnectionConfiguration);
        }

        protected IElasticLowLevelClient GetElasticClient()
        {
            return new ElasticLowLevelClient(fixture.ConnectionConfiguration);
        }
    }
}