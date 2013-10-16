using System;

namespace Hermes.Domain
{
    [Serializable]
    public abstract class DomainEvent 
    {
        public Guid EventId { get; private set; }
        public DateTime OccurredAt { get; private set; }
        public IIdentity EntityId { get; private set; }
        public IIdentity AggregateId { get; private set; }
        public int Version { get; private set; }

        protected DomainEvent()
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            AggregateId = new NullIdentity();
            EntityId = new NullIdentity();
        }

        internal void SetAggregateDetails(IAggregate aggregate)
        {
            Version = aggregate.GetVersion();
            AggregateId = aggregate.Identity;
        }

        internal void SetEntityDetails(IEntity entity)
        {
            EntityId = entity.Identity;
        }
    }
}