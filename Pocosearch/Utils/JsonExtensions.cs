using System;
using System.Text.Json;

namespace Pocosearch.Utils
{
    public static class JsonExtensions
    {
        public static object GetObject(this JsonElement element, Type objectType, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Deserialize(element.GetRawText(), objectType, options);
        }
    }
}