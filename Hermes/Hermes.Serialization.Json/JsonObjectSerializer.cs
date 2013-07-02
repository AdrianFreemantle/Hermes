using System;
using Newtonsoft.Json;

namespace Hermes.Serialization.Json
{
    /// <summary>
    /// JSON object serializer.
    /// </summary>
    public class JsonObjectSerializer : ISerializeObjects
    {
        public T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        public object DeserializeObject(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type);
        }

        public string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public string GetContentType()
        {
            return "application/json";
        }
    }
}