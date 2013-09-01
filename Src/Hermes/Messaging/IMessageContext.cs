using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public interface IMessageContext
    {
        Guid MessageId { get; }
        Guid CorrelationId { get; }
        Address ReplyToAddress { get; }
        IReadOnlyDictionary<string, string> Headers { get; }
    }
}