using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Clientele.Core.Domain
{
    [DebuggerStepThrough]
    public abstract class Aggregate : EntityBase, IAggregate
    {
        private int version;
        private readonly HashSet<IDomainEvent> changes = new HashSet<IDomainEvent>();
        private readonly HashSet<IEntity> entities = new HashSet<IEntity>();

        protected Aggregate(IHaveIdentity identity)
            : base(identity)
        {
        }

        IEnumerable<IDomainEvent> IAggregate.GetUncommittedEvents()
        {
            return changes.ToArray();
        }

        void IAggregate.ClearUncommittedEvents()
        {
            changes.Clear();
        }

        int IAggregate.GetVersion()
        {
            return version;
        }

        void IAggregate.LoadFromHistory(IEnumerable<IDomainEvent> domainEvents)
        {
            foreach (var @event in domainEvents)
            {
                ApplyEvent(@event);
                version = @event.Version;
            }
        }

        void IAggregate.RegisterOwnedEntity(IEntity entity)
        {
            RegisterOwnedEntity(entity);
        }

        protected virtual void RegisterOwnedEntity(IEntity entity)
        {
            if (entity.Identity.IsEmpty())
            {
                throw new InvalidOperationException("An entity must be assigned a non empty Identity");
            }

            if (entities.Any(e => e.Equals(entity)))
            {
                throw new InvalidOperationException(String.Format("An entity of type {0} with identity {1} is already registered.", entity.GetType().Name, entity.Identity));
            }

            entities.Add(entity);
        }

        protected override void RaiseEvent(IDomainEvent @event)
        {
            SaveEvent(@event);
            ApplyEvent(@event);
        }

        protected override void SaveEvent(IDomainEvent @event)
        {
            version++;
            @event.Version = version;
            @event.AggregateId = Identity;
            changes.Add(@event);
        }

        protected override void ApplyEvent(IDomainEvent @event)
        {
            if (@event.EntityId == null)
            {
                base.ApplyEvent(@event);
            }
            else
            {
                var entity = entities.Single(e => e.Identity.Equals(@event.EntityId));
                entity.ApplyEvent(@event);
            }
        }
    }
}
