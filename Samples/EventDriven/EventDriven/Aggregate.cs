﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EventDriven
{
    [DebuggerStepThrough]
    public abstract class Aggregate : EntityBase, IAggregate
    {
        private int version;
        private readonly HashSet<DomainEvent> changes = new HashSet<DomainEvent>();
        protected readonly HashSet<IEntity> Entities = new HashSet<IEntity>();

        protected Aggregate(IHaveIdentity identity) 
            : base(identity)
        {
        }

        IEnumerable<DomainEvent> IAggregate.GetUncommittedEvents()
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

        void IAggregate.LoadFromHistory(IEnumerable<DomainEvent> domainEvents)
        {
            foreach (var @event in domainEvents)
            {
                ApplyEvent(@event);
                version = @event.Version;
            }
        }

        internal void RegisterOwnedEntity(IEntity entity)
        {
            if (entity.Identity.IsEmpty())
            {
                throw new InvalidOperationException("An entity must be assigned a non empty Identity");
            }

            if(Entities.Any(e => e.Equals(entity)))
            {
                throw new InvalidOperationException(String.Format("Entity {0} is already registered on aggregate {1}.", entity.Identity, Identity));
            }

            Entities.Add(entity);
        }

        protected TEntity Get<TEntity>(IHaveIdentity entityId) where TEntity : IEntity
        {
            var entity = Entities.SingleOrDefault(e => e.Identity.Equals(entityId));

            if (entity == null)
            {
                throw new InvalidOperationException(String.Format("Entity {0} could not be found on aggregate {1}", entityId, Identity));
            }

            return (TEntity)entity;
        }

        protected override void RaiseEvent(DomainEvent @event)
        {
            SaveEvent(@event);
            ApplyEvent(@event);
        }

        internal override void SaveEvent(DomainEvent @event)
        {
            version++;
            @event.SetAggregateDetails(this);
            changes.Add(@event);
        }

        protected override void ApplyEvent(DomainEvent @event)
        {
            if (@event.EntityId.IsEmpty())
            {
                base.ApplyEvent(@event);
            }
            else
            {
                var entity = Entities.Single(e => e.Identity.Equals(@event.EntityId));
                entity.ApplyEvent(@event);
            }
        }

        protected TEntity RestoreEntity<TEntity>(IMemento memento) where TEntity : class, IEntity
        {
            IEntity entity = ActivatorHelper.CreateInstanceUsingNonPublicConstructor<TEntity>(this, memento.Identity);
            entity.RestoreSnapshot(memento);
            return (TEntity)entity;
        }
    }
}