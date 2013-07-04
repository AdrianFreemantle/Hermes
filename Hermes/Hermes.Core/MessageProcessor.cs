using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using Hermes.Serialization;
using Hermes.Transports;

namespace Hermes.Core
{
    public class MessageProcessor : IProcessMessage
    {
        private readonly ISerializeMessages messageSerializer;
        private readonly IDispatchMessagesToHandlers messageDispatcher;

        public MessageProcessor(ISerializeMessages messageSerializer, IDispatchMessagesToHandlers messageDispatcher)
        {
            this.messageSerializer = messageSerializer;
            this.messageDispatcher = messageDispatcher;
        }

        public bool Process(MessageEnvelope envelope)
        {
            var messages = ExtractMessages(envelope);
            messageDispatcher.DispatchToHandlers(messages);

            return true;
        }

        private IEnumerable<object> ExtractMessages(MessageEnvelope envelope)
        {
            if (envelope.Body == null || envelope.Body.Length == 0)
            {
                return new object[0];
            }

            try
            {
                using (var stream = new MemoryStream(envelope.Body))
                {
                    return messageSerializer.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                throw new SerializationException("Could not deserialize message.", e);
            }
        }
    }
}