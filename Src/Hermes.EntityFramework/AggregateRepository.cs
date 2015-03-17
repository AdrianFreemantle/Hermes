using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Attributes;
using Hermes.Domain;
using Hermes.Logging;
using Hermes.Persistence;
using Hermes.Reflection;

namespace Hermes.EntityFramework
{
    [UnitOfWorkCommitOrder(Order = 1)]
    public class AggregateRepository : IAggregateRepository, IUnitOfWork
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (AggregateRepository));

        private readonly IKeyValueStore keyValueStore;
        private readonly Dictionary<IIdentity, IAggregate> aggregateCache = new Dictionary<IIdentity, IAggregate>();
        private readonly HashSet<AggregateCommitAction> aggregateCommitActions = new HashSet<AggregateCommitAction>(); 

        public AggregateRepository(IKeyValueStore keyValueStore)
        {
            this.keyValueStore = keyValueStore;
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
            aggregateCache[aggregate.Identity] = aggregate;
            aggregateCommitActions.Add(AggregateCommitAction.Add(aggregate.Identity));
        }

        public void Update(IAggregate aggregate)
        {
            aggregateCommitActions.Add(AggregateCommitAction.Update(aggregate.Identity));
        }

        public void Remove(IAggregate aggregate)
        {
            aggregateCommitActions.Add(AggregateCommitAction.Remove(aggregate.Identity));
        }

        public void Commit()
        {
            foreach (var action in aggregateCommitActions)
            {
                ProcessCommitAction(action);
            }

            foreach (IAggregate aggregate in aggregateCache.Values)
            {
                SaveEvents(aggregate);
            }
        }

        private void SaveEvents(IAggregate aggregate)
        {
            IAggregateEvent[] events = aggregate.GetUncommittedEvents().ToArray();
            aggregate.ClearUncommittedEvents();

            //todo save events to event store
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
                    Logger.Debug("Adding aggregate {0}", action.Identity);
                    break;

                case AggregateCommitAction.CommitActionType.Update:
                    var updateSnapshot = aggregateCache[action.Identity].GetSnapshot();
                    keyValueStore.Update(action.Identity, updateSnapshot);
                    Logger.Debug("Updating aggregate {0}", action.Identity);
                    break;

                case AggregateCommitAction.CommitActionType.Remove:
                    keyValueStore.Remove(action.Identity);
                    Logger.Debug("Removing aggregate {0}", action.Identity);
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
