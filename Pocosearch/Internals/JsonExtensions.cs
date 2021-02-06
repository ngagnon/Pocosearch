using System;
using System.Text.Json;

namespace Pocosearch.Internals
{
    public static class JsonExtensions
    {
        public static object GetObject(this JsonElement element, Type objectType, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Deserialize(element.GetRawText(), objectType, options);
        }
    }
}