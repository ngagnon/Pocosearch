using System;

namespace Pocosearch
{
    public class SearchResult
    {
        public double Score { get; set; }
        public object Document { get; set; }
        public Type DocumentType { get; set; }
    }

    public class SearchResult<TDocument>
    {
        public double Score { get; set; }
        public TDocument Document { get; set; }
    }
}