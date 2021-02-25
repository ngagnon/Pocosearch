using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pocosearch.Utils;

namespace Pocosearch.Internals
{
    /* @TODO: thread safety */
    public class FullTextFieldProvider
    {
        private readonly Dictionary<Type, FullTextAttribute[]> cache = new Dictionary<Type, FullTextAttribute[]>();

        public IEnumerable<FullTextAttribute> GetFullTextFields(Type documentType)
        {
            if (!cache.TryGetValue(documentType, out var fields))
            {
                fields = FindFullTextFields(documentType).ToArray();
                cache[documentType] = fields;
            }

            return fields;
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