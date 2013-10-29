using System;

namespace Hermes.Domain.Tests
{
    public class EntityEvent : IDomainEvent
    {
        [AggregateId]
        public Guid TestAggregateId { get; protected set; }
        [EntityId]
        public int TestEntityId { get; protected set; }
        public int Version { get; protected set; }


        public string UserData { get; protected set; }

        protected EntityEvent()
        {
        }

        public EntityEvent(string userData)
        {
            UserData = userData;
        }
    }
}