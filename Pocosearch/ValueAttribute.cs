using System;

namespace Pocosearch
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ValueAttribute : Attribute
    {
        public string Name { get; set; }
    }
}