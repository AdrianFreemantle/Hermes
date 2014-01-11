using System;
using System.IO;

namespace Hermes.Messaging.Serialization
{
    public static class MessageSerializationExtensions
    {
        public static byte[] Serialize(this ISerializeMessages serializer, object message)
        {
            byte[] messageBody;

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(message, stream);
                stream.Flush();
                messageBody = stream.ToArray();
            }

            return messageBody;
        }

        public static object Deserialize(this ISerializeMessages serializer, byte[] body, Type messageType)
        {
            if (body == null || body.Length == 0)
            {
                return new object[0];
            }

            using (var stream = new MemoryStream(body))
            {
                return serializer.Deserialize(stream, messageType);
            }
        }
    }
}