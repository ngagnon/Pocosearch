using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;

namespace FultonSearch.Utils
{
    public static class BasicTokenizer
    {
        private static readonly StandardAnalyzer analyzer 
            = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

        public static IEnumerable<string> Tokenize(string text)
        {

            using (var reader = new StringReader(text))
            {
                var tokenStream = analyzer.TokenStream("foobar", reader);
                var attr = tokenStream.AddAttribute<ITermAttribute>();

                tokenStream.Reset();

                while (tokenStream.IncrementToken())
                {
                    yield return attr.Term;
                }
            }
        }
    }
}
