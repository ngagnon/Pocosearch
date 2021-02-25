using System;

namespace Pocosearch
{
    public class FullTextAttribute : Attribute
    {
        public bool SearchAsYouType { get; set; } = false;
        public string Name { get; set; }
    }
}
