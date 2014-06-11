using System;

namespace Hermes.Domain.Tests
{
    public class AggregateEvent : IAggregateEvent
    {
        [AggregateId]
        public Guid AggregateId { get; protected set; }
        public int Version { get; protected set; }
        public string UserData { get; protected set; }

        protected AggregateEvent()
        {
        }

        public AggregateEvent(string userData)
        {
            UserData = userData;
        }
    }
}