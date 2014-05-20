using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Hermes.Domain;
using Hermes.Messaging;
using Hermes.Persistence;
using Hermes.Reflection;

namespace Hermes.EntityFramework
{
    [UnitOfWorkCommitOrder(Order = 1)]
    public class AggregateRepository : IAggregateRepository, IUnitOfWork
    {
        private readonly IKeyValueStore keyValueStore;
        private readonly IInMemoryBus inMemoryBus;

        private readonly Dictionary<IIdentity, IAggregate> aggregateCache = new Dictionary<IIdentity, IAggregate>();
        private readonly HashSet<AggregateCommitAction> aggregateCommitActions = new HashSet<AggregateCommitAction>(); 

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
            PublishEvents(aggregate);
            aggregateCache[aggregate.Identity] = aggregate;
            aggregateCommitActions.Add(AggregateCommitAction.Add(aggregate.Identity));
        }

        public void Update(IAggregate aggregate)
        {
            PublishEvents(aggregate);
            aggregateCommitActions.Add(AggregateCommitAction.Update(aggregate.Identity));
        }

        public void Remove(IAggregate aggregate)
        {
            PublishEvents(aggregate);
            aggregateCommitActions.Add(AggregateCommitAction.Remove(aggregate.Identity));
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
        }

        private void PublishEvents(IEnumerable<object> events)
        {
            foreach (var e in events)
            {
                inMemoryBus.Raise(e);
            }
        }

        public void Commit()
        {
            foreach (var action in aggregateCommitActions)
            {
                ProcessCommitAction(action);
            }
        }        

        public void Rollback()
        {
            Dispose();
        }

        private void ProcessCommitAction(AggregateCommitAction action)
        {

            switch (action.ActionType)
            {
                case AggregateCommitAction.CommitActionType.Add:
                    var addSnapshot = aggregateCache[action.Identity].GetSnapshot();
                    keyValueStore.Add(action.Identity, addSnapshot);
                    break;

                case AggregateCommitAction.CommitActionType.Update:
                    var updateSnapshot = aggregateCache[action.Identity].GetSnapshot();
                    keyValueStore.Update(action.Identity, updateSnapshot);
                    break;

                case AggregateCommitAction.CommitActionType.Remove:
                    keyValueStore.Remove(action.Identity);
                    break;

                default:
                    throw new InvalidOperationException(String.Format("The commit action {0} is not handled by the repository", action.ActionType));
            }
        }

        public void Dispose()
        {
            aggregateCache.Clear();
            aggregateCommitActions.Clear();
        }
    }
}
