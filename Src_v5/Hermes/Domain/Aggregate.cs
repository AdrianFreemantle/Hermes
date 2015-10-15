using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Ioc;
using Hermes.Messaging;
using Hermes.Reflection;

namespace Hermes.Domain
{
    public abstract class Aggregate : EntityBase, IAggregate
    {
        private int version;
        private readonly HashSet<IAggregateEvent> changes = new HashSet<IAggregateEvent>();
        protected readonly HashSet<Entity> Entities = new HashSet<Entity>();
        private readonly IInMemoryBus eventBus;

        protected Aggregate(IIdentity identity) 
            : base(identity)
        {
            eventBus = ServiceLocator.Current.GetInstance<IInMemoryBus>();
        }

        IEnumerable<IAggregateEvent> IAggregate.GetUncommittedEvents()
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

        void IAggregate.LoadFromHistory(IEnumerable<IAggregateEvent> domainEvents)
        {
            foreach (var @event in domainEvents)
            {
                if (!ApplyEvent(@event, ApplyEventAs.Historical))
                {
                    throw new EventHandlerNotFoundException(this, @event);
                }

                version = @event.Version;
            }
        }

        internal void RegisterOwnedEntity(Entity entity)
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

        public TEntity Get<TEntity>(IIdentity entityId) where TEntity : Entity
        {
            var entity = Entities.SingleOrDefault(e => e.Identity.Equals(entityId));

            if (entity == null)
            {
                throw new InvalidOperationException(String.Format("Entity {0} could not be found on aggregate {1}", entityId, Identity));
            }

            return (TEntity)entity;
        }

        protected TEntity Get<TEntity>(Func<TEntity, bool> predicate) where TEntity : Entity
        {
            return GetAll<TEntity>().SingleOrDefault(predicate);
        }

        protected ICollection<TEntity> GetAll<TEntity>() where TEntity : Entity
        {
            return Entities.Where(e => e is TEntity).Cast<TEntity>().ToArray();
        }

        protected ICollection<TEntity> GetAll<TEntity>(Func<TEntity, bool> predicate) where TEntity : Entity
        {
            return GetAll<TEntity>().Where(predicate).ToArray();
        }

        internal protected override void SaveEvent(IAggregateEvent @event, EntityBase source)
        {
            version++;
            source.UpdateEventDetails(@event, this);
            changes.Add(@event);
            eventBus.Raise(@event);
        }

        internal protected override bool ApplyEvent(IAggregateEvent @event, ApplyEventAs applyAs)
        {
            if (base.ApplyEvent(@event, applyAs))
            {
                return true;
            }

            return Entities.Any(entity => entity.ApplyEvent(@event, applyAs));
        }

        protected TEntity RestoreEntity<TEntity, TIdentity>(IMemento memento, TIdentity identity) 
            where TEntity : class, IEntity
            where TIdentity : class, IIdentity
        {
            var entity = ObjectFactory.CreateInstance<TEntity>(this, identity);
            entity.RestoreSnapshot(memento);
            return entity;
        }

        void IAmRestorable.RestoreSnapshot(IMemento memento)
        {
            if (memento == null)
            {
                return;
            }

            version = memento.Version;
            RestoreSnapshot(memento);
        }

        IMemento IAmRestorable.GetSnapshot()
        {
            var snapshot = GetSnapshot();

            if (snapshot != null)
            {
                snapshot.Version = version;
                snapshot.Identity = Identity;
            }

            return snapshot;
        }
    }
}