using System;

namespace Hermes
{
    public abstract class Event : IEvent
    {
        public Guid EventId { get; private set; }
        public DateTime OccurredAt { get; private set; }

        protected Event()
        {
            EventId = SequentialGuid.New();
            OccurredAt = DateTime.UtcNow;
        }
    }
}