using System;
using Elasticsearch.Net;

namespace Pocosearch
{
    public class PocosearchException : Exception
    {
        public int? HttpStatusCode { get; private set; }

        public PocosearchException(StringResponse response) : base(response.Body)
        {
            HttpStatusCode = response.HttpStatusCode;
        }
    }
}