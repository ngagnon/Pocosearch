using System;
using Elasticsearch.Net;

namespace Pocosearch
{
    public class PocosearchException : Exception
    {
        public PocosearchException(string message) : base(message)
        {}
    }

    public class ApiException : PocosearchException
    {
        public int? HttpStatusCode { get; private set; }

        public ApiException(StringResponse response) : base(response.Body)
        {
            HttpStatusCode = response.HttpStatusCode;
        }
    }

    public class MismatchedSchemaException : PocosearchException
    {
        public MismatchedSchemaException(string indexName) 
            : base($"The POCO class schema does not match the actual Elasticsearch schema for index '{indexName}'")
        {}
    }
}