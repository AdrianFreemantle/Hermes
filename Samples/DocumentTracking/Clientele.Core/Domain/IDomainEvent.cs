using Clientele.Core.Messaging;

namespace Clientele.Core.Domain
{
    public interface IDomainEvent : IEvent
    {
        IHaveIdentity EntityId { get; set; }
        IHaveIdentity AggregateId { get; set; }
        int Version { get; set; }
    }
}