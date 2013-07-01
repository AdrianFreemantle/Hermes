using System;
using System.Collections.Generic;
using System.IO;

namespace Hermes.Serialization
{
    /// <summary>
    /// Interface for serializers that can read/write an object graph to a stream.
    /// </summary>
    public interface ISerializeMessages
    {
        /// <summary>
        /// Serializes the given set of messages into the given stream.
        /// </summary>
        /// <param name="messages">Messages to serialize.</param>
        /// <param name="stream">Stream for <paramref name="messages"/> to be serialized into.</param>
        void Serialize(object[] messages, Stream stream);

        /// <summary>
        /// Deserializes from the given stream a set of messages.
        /// </summary>
        /// <param name="stream">Stream that contains messages.</param>
        /// <param name="messageTypes">The list of message types to deserialize. If null the types must be inferred from the serialized data.</param>
        /// <returns>Deserialized messages.</returns>
        object[] Deserialize(Stream stream, IList<Type> messageTypes = null);
    }
}
