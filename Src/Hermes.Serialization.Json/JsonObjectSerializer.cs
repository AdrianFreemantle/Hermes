using System;
using System.Globalization;
using System.Runtime.Serialization.Formatters;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
            DefaultValueHandling = DefaultValueHandling.Populate,
            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.RoundtripKind }, new XContainerConverter() }
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