using System;

namespace Hermes.Messaging
{
    public interface IMessageContext
    {
        Guid MessageId { get; }
        Address ReplyToAddress { get; }
        Address Destination { get; }
        string UserName { get; }
        Guid CorrelationId { get; }
    }
}