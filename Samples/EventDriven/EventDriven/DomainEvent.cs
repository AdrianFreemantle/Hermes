using System;
using System.Diagnostics;

namespace EventDriven
{
    [DebuggerStepThrough]
    public class NullIdentity : IHaveIdentity
    {
        private const string NullId = "Null ID";

        public dynamic GetId()
        {
            return string.Empty;
        }

        public bool IsEmpty()
        {
            return true;
        }

        public string GetTag()
        {
            return String.Empty;
        }

        public override string ToString()
        {
            return NullId;
        }
    }

    [Serializable]
    [DebuggerStepThrough]
    public abstract class DomainEvent 
    {
        public Guid EventId { get; private set; }
        public DateTime OccurredAt { get; private set; }
        public IHaveIdentity EntityId { get; private set; }
        public IHaveIdentity AggregateId { get; private set; }
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