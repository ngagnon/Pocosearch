using System;
using System.Collections.Generic;
using System.Linq;
using Pocosearch.Utils;

namespace Pocosearch.Internals
{
    /* @TODO: thread safety */
    public class FullTextFieldProvider
    {
        private readonly Dictionary<Type, string[]> cache = new Dictionary<Type, string[]>();

        public IEnumerable<string> GetFullTextFields(Type documentType)
        {
            if (!cache.TryGetValue(documentType, out var fields))
            {
                fields = FindFullTextFields(documentType).ToArray();
                cache[documentType] = fields;
            }

            return fields;
        }

        private static IEnumerable<string> FindFullTextFields(Type documentType)
        {
            return documentType.GetProperties()
                .Where(p => p.GetCustomAttribute<FullTextAttribute>() != null)
                .Select(p => p.Name);
        }
    }
}