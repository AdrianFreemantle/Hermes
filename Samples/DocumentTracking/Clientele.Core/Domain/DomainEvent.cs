using System;

using Clientele.Core.Messaging;

namespace Clientele.Core.Domain
{
    public abstract class DomainEvent : Event, IDomainEvent
    {
        public IHaveIdentity EntityId { get; set; }
        public IHaveIdentity AggregateId { get; set; }
        public int Version { get; set; }
    }
}