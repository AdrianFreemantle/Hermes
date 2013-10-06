using System;

namespace Hermes
{
    public interface IEvent : IMessage
    {
        Guid EventId { get; }
        DateTime OccurredAt { get; }
    }
}