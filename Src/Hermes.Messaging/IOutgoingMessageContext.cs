using System;
using System.Collections.Generic;

using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    public interface IOutgoingMessageContext : IMessageContext
    {
        IEnumerable<Type> GetMessageContracts();
        void AddMessage(object message);
        void SetMessageId(Guid id);
        void SetCorrelationId(Guid id);
        void SetReplyAddress(Address address);
        void AddHeader(HeaderValue headerValue);
        TransportMessage ToTransportMessage();
        TransportMessage ToTransportMessage(TimeSpan timeToLive);
    }
}