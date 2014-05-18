using System.Collections.Generic;
using System.Linq;
using Hermes.Domain;
using Hermes.Messaging;
using Hermes.Persistence;
using Hermes.Reflection;

namespace Hermes.EntityFramework
{
    public class AggregateRepository : IAggregateRepository
    {
        private readonly IKeyValueStore keyValueStore;
        private readonly IInMemoryBus inMemoryBus;

        private readonly Dictionary<IIdentity, IAggregate> aggregateCache = new Dictionary<IIdentity, IAggregate>();

        public AggregateRepository(IKeyValueStore keyValueStore, IInMemoryBus inMemoryBus)
        {
            this.keyValueStore = keyValueStore;
            this.inMemoryBus = inMemoryBus;
        }

        public TAggregate Get<TAggregate>(IIdentity id) where TAggregate : class, IAggregate
        {
            if (aggregateCache.ContainsKey(id))
            {
                return (TAggregate)aggregateCache[id];
            }

            var memento = keyValueStore.Get(id) as IMemento;
            var aggregate = ObjectFactory.CreateInstance<TAggregate>(id);
            aggregate.RestoreSnapshot(memento);
            aggregateCache.Add(id, aggregate);

            return aggregate;
        }

        public void Add(IAggregate aggregate)
        {
            IMemento memento = aggregate.GetSnapshot();
            PublishEvents(aggregate);
            keyValueStore.Add(memento.Identity, memento);
        }

        public void Update(IAggregate aggregate)
        {
            IMemento memento = aggregate.GetSnapshot();
            PublishEvents(aggregate);
            keyValueStore.Update(memento.Identity, memento);
        }

        public void Remove(IAggregate aggregate)
        {
            PublishEvents(aggregate);
            keyValueStore.Remove(aggregate.Identity);
        }

        private void PublishEvents(IAggregate aggregate)
        {
            object[] events;

            do
            {
                events = aggregate.GetUncommittedEvents().Cast<object>().ToArray();
                aggregate.ClearUncommittedEvents();
                PublishEvents(events);

            } while (events.Any());

            PublishEvents(events);
        }

        private void PublishEvents(IEnumerable<object> events)
        {
            foreach (var e in events)
            {
                inMemoryBus.Raise(e);
            }
        }
    }
}
