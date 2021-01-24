using System;
using System.Collections.Generic;

namespace FultonSearch.Utils
{
    public static class EdgeNGram
    {
        public static IEnumerable<string> Generate(string token, int minGram, int maxGram)
        {
            yield return token;

            for (var i = minGram; i <= maxGram && i <= token.Length; i++)
            {
                yield return token.Substring(0, i);
            }
        }
    }
}
