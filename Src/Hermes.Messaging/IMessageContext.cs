using System;
using System.Collections.Generic;

using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    public interface IMessageContext 
    {
        Guid MessageId { get; }
        Guid CorrelationId { get; }
        Address ReplyToAddress { get; }
        IEnumerable<HeaderValue> Headers { get; }
    }
}