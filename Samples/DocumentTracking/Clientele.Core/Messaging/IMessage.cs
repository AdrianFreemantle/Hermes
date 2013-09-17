using System;

namespace Clientele.Core.Messaging
{
    public interface IMessage
    {
        Guid MessageId { get; }
        DateTime OccurredAt { get; }
    }
}