using System;

namespace Hermes.Messaging
{
    public interface IMessageContext 
    {
        Guid MessageId { get; }
        Guid CorrelationId { get; }
        string UserName { get; }
        Address ReplyToAddress { get; }
    }
}