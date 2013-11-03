using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Hermes.Messaging.Bus.Transports;

namespace Hermes.Messaging.Bus
{
    public class MessageContext : IMessageContext
    {
        private readonly TransportMessage transportMessage;
        private readonly ReadOnlyDictionary<string, string> headers;

        public MessageContext(TransportMessage transportMessage)
        {
            this.transportMessage = transportMessage;
            headers = new ReadOnlyDictionary<string, string>(transportMessage.Headers);
        }

        public Guid MessageId
        {
            get { return transportMessage.MessageId; }
        }

        public Guid CorrelationId
        {
            get { return transportMessage.CorrelationId; }
        }

        public Address ReplyToAddress
        {
            get { return transportMessage.ReplyToAddress; }
        }

        public IReadOnlyDictionary<string, string> Headers
        {
            get { return headers; }
        }
    }
}