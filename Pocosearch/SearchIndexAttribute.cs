using System;

namespace Pocosearch
{
    ///
    /// Can be used to give a unique name to a search index.
    /// By default, the name is the fully qualified name of the class, including assembly.
    ///
    public class SearchIndexAttribute : Attribute
    {
        public string Name { get; set; }

        public SearchIndexAttribute(string name)
        {
            Name = name;
        }
    }
}