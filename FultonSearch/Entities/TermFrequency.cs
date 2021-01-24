using System;

namespace FultonSearch.Entities
{
    public class TermFrequency
    {
        public int IndexId { get; set; }
        public string Token { get; set; }
        public int DocumentID { get; set; }
        public double Frequency { get; set; }
    }
}
