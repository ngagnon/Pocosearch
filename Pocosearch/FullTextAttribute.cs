using System;

namespace Pocosearch
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FullTextAttribute : Attribute
    {
        public bool SearchAsYouType { get; set; } = false;
        public string Name { get; set; }
    }
}
