using System.Linq;
using Hermes.Domain;
using Hermes.Messaging;

namespace Hermes.EntityFramework
{
    public class AggregateRepository : IAggregateRepository
    {
        private readonly IKeyValueStore keyValueStore;
        private readonly IMessageBus messageBus;
        private readonly IInMemoryBus inMemoryBus;

        public AggregateRepository(IKeyValueStore keyValueStore, IMessageBus messageBus, IInMemoryBus inMemoryBus)
        {
            this.keyValueStore = keyValueStore;
            this.messageBus = messageBus;
            this.inMemoryBus = inMemoryBus;
        }

        public TAggregate Get<TAggregate>(IIdentity id) where TAggregate : class, IAggregate
        {
            var memento = keyValueStore.Get(id) as IMemento;
            var aggregate = ObjectFactory.CreateInstance<TAggregate>(id);
            aggregate.RestoreSnapshot(memento);
            return aggregate;
        }

        public void Add(IAggregate aggregate)
        {
            IMemento memento = aggregate.GetSnapshot();
            keyValueStore.Add(memento.Identity, memento);
            PublishEvents(aggregate);
        }

        public void Update(IAggregate aggregate)
        {
            IMemento memento = aggregate.GetSnapshot();
            keyValueStore.Update(memento.Identity, memento);
            PublishEvents(aggregate);
        }

        private void PublishEvents(IAggregate aggregate)
        {
            var events = aggregate.GetUncommittedEvents().Cast<object>().ToArray();
            inMemoryBus.Raise(events);
            messageBus.Publish(events);
            aggregate.ClearUncommittedEvents();
        }
    }
}
