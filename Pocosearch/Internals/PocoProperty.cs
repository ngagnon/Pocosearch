using System;
using System.Reflection;

namespace Pocosearch.Internals
{
    internal class PocoProperty
    {
        /* Property name (in C# source code) */
        public string Name { get; set; }

        /* Field name (in Elasticsearch index) */
        public string FieldName { get; set; }

        public bool IsFullText { get; set; }
        public bool SearchAsYouType { get; set; }
        public Type Type { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public bool Ignored { get; set; }

        public PocoProperty()
        {}

        public PocoProperty(PropertyInfo prop)
        {
            PropertyInfo = prop;
            Name = prop.Name;
            Type = prop.PropertyType;

            var ignoreAttribute = prop.GetCustomAttribute<IgnoreAttribute>();
            var fullTextAttribute = prop.GetCustomAttribute<FullTextAttribute>();
            var valueAttribute = prop.GetCustomAttribute<ValueAttribute>();
            var documentIdAttribute = prop.GetCustomAttribute<DocumentIdAttribute>();

            if (fullTextAttribute != null && valueAttribute != null)
                throw new ArgumentException($"Property {prop.Name}: cannot have both [Value] and [FullText] attributes.", nameof(prop));

            if (fullTextAttribute != null && ignoreAttribute != null)
                throw new ArgumentException($"Property {prop.Name}: cannot have both [Ignore] and [FullText] attributes.", nameof(prop));

            if (valueAttribute != null && ignoreAttribute != null)
                throw new ArgumentException($"Property {prop.Name}: cannot have both [Ignore] and [Value] attributes.", nameof(prop));

            if (documentIdAttribute != null && ignoreAttribute != null)
                throw new ArgumentException($"Property {prop.Name}: cannot have both [Ignore] and [DocumentId] attributes.", nameof(prop));

            if (fullTextAttribute != null)
                SearchAsYouType = fullTextAttribute.SearchAsYouType;

            if (fullTextAttribute != null && !string.IsNullOrEmpty(fullTextAttribute.Name))
                FieldName = fullTextAttribute.Name;
            else if (valueAttribute != null && !string.IsNullOrEmpty(valueAttribute.Name))
                FieldName = valueAttribute.Name;
            else
                FieldName = prop.Name;
            
            Ignored = ignoreAttribute != null;
            IsFullText = fullTextAttribute != null;
        }
    }
}