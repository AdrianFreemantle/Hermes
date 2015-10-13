using Hermes.Messaging;

namespace Hermes.Domain
{
    public interface IAggregateEvent : IDomainEvent
    {
        int Version { get; }
    }
}