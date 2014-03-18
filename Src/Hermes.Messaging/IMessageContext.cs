using System;
using System.Collections.Generic;

using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    public interface IMessageContext 
    {
        Guid MessageId { get; }
        Guid CorrelationId { get; }
        Guid UserId { get; }
        Address ReplyToAddress { get; }
    }
}