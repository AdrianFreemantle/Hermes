using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Hermes.Reflection;

namespace Hermes.Domain
{
    //[DebuggerStepThrough]
    public abstract class Aggregate : EntityBase, IAggregate
    {
        private int version;
        private readonly HashSet<IDomainEvent> changes = new HashSet<IDomainEvent>();
        protected readonly HashSet<Entity> Entities = new HashSet<Entity>();

        protected Aggregate(IIdentity identity) 
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

        protected TEntity Get<TEntity>(IIdentity entityId) where TEntity : Entity
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

        internal protected override void SaveEvent(IDomainEvent @event, EntityBase source)
        {
            version++;
            source.UpdateEventDetails(@event, this);
            changes.Add(@event);
        }

        internal protected override bool ApplyEvent(IDomainEvent @event, ApplyEventAs applyAs)
        {
            if (base.ApplyEvent(@event, applyAs))
            {
                return true;
            }

            return Entities.Any(entity => entity.ApplyEvent(@event, applyAs));
        }

        protected TEntity RestoreEntity<TEntity>(IMemento memento) where TEntity : class, IEntity
        {
            var entity = ObjectFactory.CreateInstance<TEntity>(this, memento.Identity);
            entity.RestoreSnapshot(memento);
            return entity;
        }
    }
}