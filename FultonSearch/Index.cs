using System;
using System.Collections.Generic;
using System.Data;
using FultonSearch.Utils;

namespace FultonSearch
{
    public abstract class Index
    {
        public abstract int ID { get; }
        public virtual string ScanQuery { get; } = null;

        public virtual IEnumerable<string> Tokenize(string text)
        {
            return BasicTokenizer.Tokenize(text);
        }
    }
}
