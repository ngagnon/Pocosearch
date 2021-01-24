using System;
using System.Collections.Generic;
using System.Linq;

namespace FultonSearch.Entities
{
    public class SearchData
    {
        public int NumDocs { get; set; }
        public List<TermFrequency> Results { get; set; }
        public List<IGrouping<int, TermFrequency>> ResultsByDoc { get; set; }
        public Dictionary<int, double> Norms { get; set; }
    }
}
