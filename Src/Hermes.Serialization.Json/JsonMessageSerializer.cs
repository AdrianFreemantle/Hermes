using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Text;

using Hermes.Messaging;
using Hermes.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hermes.Serialization.Json
{
    public class JsonMessageSerializer : ISerializeMessages
    {
        private readonly ITypeMapper typeMapper;

        readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.RoundtripKind }, new XContainerConverter() }
        };

        /// <summary>
        /// Gets the content type into which this serializer serializes the content to 
        /// </summary>
        public string ContentType { get { return GetContentType(); } }

        public JsonMessageSerializer(ITypeMapper typeMapper)
        {
            this.typeMapper = typeMapper;
        }

        /// <summary>
        /// Serializes the given set of messages into the given stream.
        /// </summary>
        /// <param name="message">Message to serialize.</param>
        /// <param name="stream">Stream for <paramref name="message"/> to be serialized into.</param>
        public void Serialize(object message, Stream stream)
        {
            var jsonSerializer = JsonSerializer.Create(serializerSettings);
            jsonSerializer.Binder = new MessageSerializationBinder(typeMapper);

            var jsonWriter = CreateJsonWriter(stream);
            jsonSerializer.Serialize(jsonWriter, message);

            jsonWriter.Flush();
        }

        /// <summary>
        /// Deserializes from the given stream a set of messages.
        /// </summary>
        /// <param name="stream">Stream that contains messages.</param>
        /// <param name="messageType">The list of message types to deserialize. If null the types must be inferred from the serialized data.</param>
        /// <returns>Deserialized messages.</returns>
        public object Deserialize(Stream stream, Type messageType)
        {
            JsonSerializer jsonSerializer = JsonSerializer.Create(serializerSettings);
            jsonSerializer.ContractResolver = new MessageContractResolver(typeMapper);

            var reader = CreateJsonReader(stream);
            reader.Read();

            if (messageType != null)
            {
                return jsonSerializer.Deserialize(reader, messageType);
            }

            return jsonSerializer.Deserialize<object>(reader);
        }        

        protected virtual JsonWriter CreateJsonWriter(Stream stream)
        {
            var streamWriter = new StreamWriter(stream, Encoding.UTF8);
            return new JsonTextWriter(streamWriter) { Formatting = Formatting.None };
        }

        protected virtual JsonReader CreateJsonReader(Stream stream)
        {
            var streamReader = new StreamReader(stream, Encoding.UTF8);
            return new JsonTextReader(streamReader);
        }

        protected virtual string GetContentType()
        {
            return ContentTypes.Json;
        }
    }
}
