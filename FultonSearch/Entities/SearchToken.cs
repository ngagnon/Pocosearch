using System;

namespace FultonSearch.Entities
{
    public class SearchToken
    {
        public string Value { get; set; }
        public double Boost { get; set; }

        public override bool Equals(object obj)
        {
            return obj is SearchToken token 
                && Value == token.Value
                && Boost == token.Boost;
        }

        public override int GetHashCode()
        {
            return new { Value, Boost }.GetHashCode();
        }
    }
}
