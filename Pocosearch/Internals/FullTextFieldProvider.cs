using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pocosearch.Utils;

namespace Pocosearch.Internals
{
    public class FullTextFieldProvider
    {
        private readonly ConcurrentDictionary<Type, FullTextAttribute[]> cache 
            = new ConcurrentDictionary<Type, FullTextAttribute[]>();

        public IEnumerable<FullTextAttribute> GetFullTextFields(Type documentType)
        {
            return cache.GetOrAdd(documentType, key => FindFullTextFields(key).ToArray());
        }

        private static IEnumerable<FullTextAttribute> FindFullTextFields(Type documentType)
        {
            return documentType.GetProperties()
                .Select(p => new { Property = p, Attribute = p.GetCustomAttribute<FullTextAttribute>() })
                .Where(x => x.Attribute != null)
                .Select(x => FillName(x.Attribute, x.Property));
        }

        private static FullTextAttribute FillName(FullTextAttribute attribute, PropertyInfo prop)
        {
            if (string.IsNullOrEmpty(attribute.Name))
                attribute.Name = prop.Name;

            return attribute;
        }
    }
}