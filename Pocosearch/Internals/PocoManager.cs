using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Pocosearch.Utils;

namespace Pocosearch.Internals
{
    internal class PocoManager
    {
        private readonly ConcurrentDictionary<Type, PocoInfo> cache
            = new ConcurrentDictionary<Type, PocoInfo>();

        public Dictionary<string, object> Serialize<T>(T document)
        {
            var properties = GetPocoProperties(typeof(T));
            var serialized = new Dictionary<string, object>();

            foreach (var property in properties)
                serialized[property.FieldName] = property.PropertyInfo.GetValue(document);

            return serialized;
        }

        public object Deserialize(JsonElement json, Type documentType)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = GetPocoInfo(documentType).NamingPolicy
            };

            return json.GetObject(documentType, options);
        }

        public PocoProperty GetPocoProperty(Type documentType, string name)
        {
            var properties = GetPocoProperties(documentType);
            return properties.First(x => x.Name == name);
        }

        public IEnumerable<PocoProperty> GetPocoProperties(Type documentType)
        {
            return GetPocoInfo(documentType).Properties;
        }

        private PocoInfo GetPocoInfo(Type documentType)
        {
            return cache.GetOrAdd(documentType, t => ExtractPocoInfo(t));
        }

        private PocoInfo ExtractPocoInfo(Type documentType)
        {
            var properties = documentType.GetProperties();
            var info = new PocoInfo
            {
                Properties = new List<PocoProperty>()
            };

            foreach (var property in properties)
            {
                var pocoProp = new PocoProperty(property);

                if (!pocoProp.Ignored)
                    info.Properties.Add(pocoProp);
            }

            info.NamingPolicy = new FieldMappingPolicy(info.Properties);

            return info;
        }

        private class PocoInfo
        {
            public JsonNamingPolicy NamingPolicy { get; set; }
            public List<PocoProperty> Properties { get; set; }
        }

        private class FieldMappingPolicy : JsonNamingPolicy
        {
            private readonly Dictionary<string, string> mappings;

            public FieldMappingPolicy(IEnumerable<PocoProperty> properties)
            {
                mappings = properties.ToDictionary(x => x.Name, x => x.FieldName);
            }

            public override string ConvertName(string name)
            {
                return mappings.ContainsKey(name) ? mappings[name] : name;
            }
        }
    }
}