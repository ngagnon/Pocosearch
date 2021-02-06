using System;
using Elasticsearch.Net;

namespace Pocosearch
{
    /* @TODO: need to subclass connection polls, and auth credentials classes */
    public class ConnectionConfiguration : Elasticsearch.Net.ConnectionConfiguration
    {
        public ConnectionConfiguration(Uri uri = null) : base(uri)
        {}

        public ConnectionConfiguration(IConnectionPool connectionPool) : base(connectionPool)
        {}

        public ConnectionConfiguration(string cloudId, BasicAuthenticationCredentials credentials)
            : base(cloudId, credentials)
        {}

        public ConnectionConfiguration(string cloudId, ApiKeyAuthenticationCredentials credentials)
            : base(cloudId, credentials)
        {}
    }
}