using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Elastic.Elasticsearch.Ephemeral;
using Elasticsearch.Net;
using Xunit;

[assembly: TestFramework("Pocosearch.Tests.Framework.XunitTestFrameworkWithAssemblyFixture", "Pocosearch.Tests")]
[assembly: AssemblyFixture(typeof(Pocosearch.Tests.Framework.ElasticsearchClusterFixture))]

namespace Pocosearch.Tests.Framework
{
    public class ElasticsearchClusterFixture : IDisposable
    {
        private readonly ElasticsearchSettings settings;
        private readonly EphemeralCluster cluster;
        private readonly ConnectionConfiguration connectionConfig;

        public ElasticsearchClusterFixture()
        {
            Uri[] nodes;

            settings = Configuration.Get<ElasticsearchSettings>();

            if (settings.Managed)
            {
                cluster = new EphemeralCluster("7.12.0");
                cluster.Start();

                nodes = cluster.NodesUris().ToArray();
            }
            else
            {
                nodes = new Uri[] { new Uri($"http://{settings.Host}:{settings.Port}") };
            }

            var connectionPool = new StaticConnectionPool(nodes);
            connectionConfig = new ConnectionConfiguration(connectionPool);
        }

        public ConnectionConfiguration ConnectionConfiguration => connectionConfig;

        public void Dispose()
        {
            if (settings.Managed)
                cluster.Dispose();
        }
    }
}