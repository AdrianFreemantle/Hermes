using System;
using System.Collections.Generic;

namespace Hermes.Messaging.Bus.Transports
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

        public TransportMessage BuildTransportMessage(Guid correlationId, object[] messages)
        {
            return BuildTransportMessage(correlationId, TimeSpan.MaxValue, messages);
        }

        public TransportMessage BuildTransportMessage(Guid correlationId, TimeSpan timeToLive, object[] messages)
        {
            return BuildTransportMessage(correlationId, timeToLive, messages, new Dictionary<string, string>());
        }

        public TransportMessage BuildTransportMessage(Guid correlationId, TimeSpan timeToLive, object[] messages, IDictionary<string, string> headers)
        {
            var messageBody = messageSerializer.Serialize(messages);
            return new TransportMessage(SequentialGuid.New(), correlationId, Address.Local, timeToLive, headers, messageBody);
        }

        public TransportMessage BuildControlMessage(Guid correlationId, IEnumerable<HeaderValue> headerValues)
        {
            var controlHeaders = BuildControlMessageHeaders(headerValues);
            return new TransportMessage(SequentialGuid.New(), correlationId, controlHeaders);
        }

        private static Dictionary<string, string> BuildControlMessageHeaders(IEnumerable<HeaderValue> headerValues)
        {
            var controlHeaders = new Dictionary<string, string>
            {
                {HeaderKeys.ControlMessageHeader, true.ToString()}
            };

            foreach (var header in headerValues)
            {
                controlHeaders.Add(header.Key, header.Value);
            }

            return controlHeaders;
        }     
    }
}