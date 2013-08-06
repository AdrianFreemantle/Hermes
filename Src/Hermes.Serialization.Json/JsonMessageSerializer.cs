using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Hermes.Serialization.Json
{
  public class JsonMessageSerializer : ISerializeMessages
    {
        private readonly Encoding encoding = Encoding.UTF8;

        readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
            TypeNameHandling = TypeNameHandling.Auto,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.RoundtripKind }, new XContainerConverter() }
        };

        readonly JsonSerializerSettings serializerSettings2 = new JsonSerializerSettings
        {
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
            TypeNameHandling = TypeNameHandling.All,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.RoundtripKind }, new XContainerConverter() }
        };

        public virtual void Serialize(object[] messages, Stream stream)
        {
            if (messages.Length == 1)
            {
                var jsonSerializer = JsonSerializer.Create(serializerSettings2);
                jsonSerializer.Binder = new MessageSerializationBinder();
                var jsonWriter = CreateJsonWriter(stream);
                jsonSerializer.Serialize(jsonWriter, messages[0]);
                jsonWriter.Flush();
            }
            else
            {
                var jsonSerializer = JsonSerializer.Create(serializerSettings);
                jsonSerializer.Binder = new MessageSerializationBinder();
                var jsonWriter = CreateJsonWriter(stream);
                jsonSerializer.Serialize(jsonWriter, messages);
                jsonWriter.Flush();
            }
        }

        public virtual object[] Deserialize(Stream stream)
        {
            JsonSerializer jsonSerializer = JsonSerializer.Create(serializerSettings);

            var reader = CreateJsonReader(stream);
            reader.Read();
            var firstTokenType = reader.TokenType;

            if (firstTokenType == JsonToken.StartArray)
            {
                return jsonSerializer.Deserialize<object[]>(reader);
            }
            
            return new[] { jsonSerializer.Deserialize<object>(reader)};
        }

        protected virtual JsonWriter CreateJsonWriter(Stream stream)
        {
            var streamWriter = new StreamWriter(stream, encoding);
            return new JsonTextWriter(streamWriter) { Formatting = Formatting.None };
        }

        protected virtual JsonReader CreateJsonReader(Stream stream)
        {
            var streamReader = new StreamReader(stream, encoding);
            return new JsonTextReader(streamReader);
        }
    }
}
