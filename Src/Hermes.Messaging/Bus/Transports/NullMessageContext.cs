using System;
using System.Collections.Generic;

namespace Hermes.Messaging.Bus.Transports
{
    public class NullMessageContext : IMessageContext
    {
        static readonly HeaderValue[] EmptyHeaders = new HeaderValue[0];

        public Guid MessageId { get { return Guid.Empty; } }
        public Guid CorrelationId { get { return Guid.Empty; } }
        public Address ReplyToAddress { get { return Address.Parse("__UNDEFINED"); } }
        public IEnumerable<HeaderValue> Headers { get { return EmptyHeaders; } }
    }
}