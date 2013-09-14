using System;
using System.Collections.Generic;
using System.IO;
using Hermes.Messaging;
using Hermes.Serialization;

namespace Hermes.Core
{
    public class TransportMessageFactory : ITransportMessageFactory
    {
        private readonly ISerializeMessages messageSerializer;

        public TransportMessageFactory(ISerializeMessages messageSerializer)
        {
            this.messageSerializer = messageSerializer;
        }

        public TransportMessage BuildTransportMessage(object[] messages)
        {
            return BuildTransportMessage(Guid.Empty, TimeSpan.MaxValue, messages);
        }

        public TransportMessage BuildTransportMessage(Guid correlationId, TimeSpan timeToLive, object[] messages)
        {
            return BuildTransportMessage(correlationId, timeToLive, messages, new Dictionary<string, string>());
        }

        public TransportMessage BuildTransportMessage(Guid correlationId, TimeSpan timeToLive, object[] messages, IDictionary<string, string> headers)
        {
            var messageBody = SerializeMessages(messages);
            return new TransportMessage(IdentityFactory.NewComb(), correlationId, Address.Local, timeToLive, headers, messageBody);
        }

        public TransportMessage BuildControlMessage(Guid correlationId, IEnumerable<HeaderValue> headerValues)
        {
            var controlHeaders = BuildControlMessageHeaders(headerValues);
            return new TransportMessage(IdentityFactory.NewComb(), correlationId, controlHeaders);
        }

        private static Dictionary<string, string> BuildControlMessageHeaders(IEnumerable<HeaderValue> headerValues)
        {
            var controlHeaders = new Dictionary<string, string>
            {
                {Headers.ControlMessageHeader, true.ToString()}
            };

            foreach (var header in headerValues)
            {
                controlHeaders.Add(header.Key, header.Value);
            }

            return controlHeaders;
        }

        private byte[] SerializeMessages(object[] messages)
        {
            byte[] messageBody;

            using (var stream = new MemoryStream())
            {
                messageSerializer.Serialize(messages, stream);
                stream.Flush();
                messageBody = stream.ToArray();
            }
            return messageBody;
        }        
    }
}