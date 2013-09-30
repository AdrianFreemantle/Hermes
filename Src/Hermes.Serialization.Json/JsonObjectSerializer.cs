using System;
using System.Runtime.Serialization.Formatters;

using Newtonsoft.Json;

namespace Hermes.Serialization.Json
{
    /// <summary>
    /// JSON object serializer.
    /// </summary>
    public class JsonObjectSerializer : ISerializeObjects
    {
        static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
            TypeNameHandling = TypeNameHandling.All,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            MissingMemberHandling = MissingMemberHandling.Error,
            DefaultValueHandling = DefaultValueHandling.Populate,
            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
        };

        public T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, SerializerSettings);
        }

        public object DeserializeObject(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type, SerializerSettings);
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