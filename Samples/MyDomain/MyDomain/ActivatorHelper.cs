using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace MyDomain
{
    public interface ICommand
    {
        Guid CommandId { get; }
    }

    public abstract class Command : ICommand
    {
        public Guid CommandId { get; private set; }

        protected Command()
        {
            CommandId = Guid.NewGuid();
        }
    }

    public abstract class TypedAggregate<TAggregateState> : Aggregate where TAggregateState : class, new()
    {
        protected TAggregateState State;

        protected TypedAggregate(IHaveIdentity identity)
            : base(identity)
        {
            State = new TAggregateState();
        }

        protected override void ApplyEvent(IDomainEvent @event)
        {
            ((dynamic)State).When((dynamic)@event);
        }
    }

    public abstract class RestorableTypedAggregate<TAggregateState> : TypedAggregate<TAggregateState> where TAggregateState : class, IMemento, new()
    {
        protected RestorableTypedAggregate(IHaveIdentity identity)
            : base(identity)
        {
            State.Identity = identity;
        }

        protected override void RestoreSnapshot(IMemento memento)
        {
            State = (TAggregateState)memento;
        }

        protected override IMemento GetSnapshot()
        {
            return State;
        }
    }

    public interface IMemento
    {
        IHaveIdentity Identity { get; set; }
    }

    public interface IHaveIdentity
    {
        dynamic GetId();
        bool IsEmpty();
        string GetTag();
    }

    public interface IDomainEvent
    {
        Guid EventId { get; }
        DateTime OccurredAt { get; }
        IHaveIdentity EntityId { get; set; }
        IHaveIdentity AggregateId { get; set; }
        int Version { get; set; }
    }

    public interface IEntity : IAmRestorable
    {
        IHaveIdentity Identity { get; }
        void ApplyEvent(IDomainEvent @event);
        void SaveEvent(IDomainEvent @event);
    }

    [DataContract]
    [DebuggerStepThrough]
    public abstract class Identity : IEquatable<Identity>, IHaveIdentity
    {
        private static readonly Type[] SupportTypes = { typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(Guid), typeof(string) };

        [DataMember]
        protected dynamic Id { get; private set; }

        public virtual string GetTag()
        {
            var typeName = GetType().Name;

            return typeName.EndsWith("Id")
                ? typeName.Substring(0, typeName.Length - 2)
                : typeName;
        }

        protected Identity(dynamic id)
        {
            VerifyIdentityType(id);
            Id = id;
        }

        public dynamic GetId()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            var identity = obj as Identity;

            return identity != null && Equals(identity);
        }

        public bool Equals(Identity other)
        {
            if (other != null)
            {
                return other.Id.Equals(Id) && other.GetTag() == GetTag();
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", GetTag(), Id);
        }

        public override int GetHashCode()
        {
            return (Id.GetHashCode());
        }

        void VerifyIdentityType(dynamic id)
        {
            if (id == null)
            {
                throw new ArgumentException("You must provide a non null value as an identity");
            }

            var type = id.GetType();

            if (SupportTypes.Any(t => t == type))
            {
                return;
            }

            throw new InvalidOperationException("Abstract identity inheritors must provide stable hash. It is not supported for:  " + type);
        }

        public virtual bool IsEmpty()
        {
            var type = Id.GetType();

            if (type == typeof(string))
            {
                return Id == string.Empty;
            }

            return Activator.CreateInstance(type) == Id;
        }

        public static bool operator ==(Identity left, Identity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Identity left, Identity right)
        {
            return !Equals(left, right);
        }
    }

    public interface IAmRestorable
    {
        IMemento GetSnapshot();
        void RestoreSnapshot(IMemento memento);
    }

    public interface IAggregate : IEntity
    {
        IEnumerable<IDomainEvent> GetUncommittedEvents();
        void ClearUncommittedEvents();
        int GetVersion();
        void LoadFromHistory(IEnumerable<IDomainEvent> domainEvents);
        void RegisterOwnedEntity(IEntity entity);
    }

    [DebuggerStepThrough]
    public abstract class EntityBase : IEntity
    {
        public IHaveIdentity Identity { get; set; }
        private readonly Dictionary<Type, Action<object>> eventHandlers;

        protected EntityBase(IHaveIdentity identity)
        {
            Identity = identity;
            eventHandlers = new Dictionary<Type, Action<object>>();
            GetEventHandlers();
        }

        protected virtual void RaiseEvent(IDomainEvent @event)
        {
            ApplyEvent(@event);
            SaveEvent(@event);
        }

        void IEntity.ApplyEvent(IDomainEvent @event)
        {
            ApplyEvent(@event);
        }

        protected virtual void ApplyEvent(IDomainEvent @event)
        {
            eventHandlers[@event.GetType()].Invoke(@event);
        }

        void IEntity.SaveEvent(IDomainEvent @event)
        {
            SaveEvent(@event);
        }

        protected abstract void SaveEvent(IDomainEvent @event);

        public override int GetHashCode()
        {
            return Identity.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EntityBase);
        }

        public virtual bool Equals(EntityBase other)
        {
            if (null != other && other.GetType() == GetType())
            {
                return other.Identity.Equals(Identity);
            }

            return false;
        }

        public static bool operator ==(EntityBase left, EntityBase right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityBase left, EntityBase right)
        {
            return !Equals(left, right);
        }

        void IAmRestorable.RestoreSnapshot(IMemento memento)
        {
            if (memento == null)
            {
                return;
            }

            RestoreSnapshot(memento);
        }

        protected virtual void RestoreSnapshot(IMemento memento)
        {
            throw new NotImplementedException("The entity does not currently support restoring from a snapshot");
        }

        IMemento IAmRestorable.GetSnapshot()
        {
            var snapshot = GetSnapshot();

            if (snapshot != null)
            {
                snapshot.Identity = Identity;
            }

            return snapshot;
        }

        protected virtual IMemento GetSnapshot()
        {
            return null;
        }

        public override string ToString()
        {
            return Identity.ToString();
        }

        [DebuggerStepThrough]
        private void GetEventHandlers()
        {
            Type EventBaseType = typeof(IDomainEvent);

            var targetType = GetType();

            var methodsToMatch = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var matchedMethods = from method in methodsToMatch
                                 let parameters = method.GetParameters()
                                 where
                                     method.Name.Equals("When", StringComparison.InvariantCulture) &&
                                         parameters.Length == 1 &&
                                         EventBaseType.IsAssignableFrom(parameters[0].ParameterType)
                                 select
                                     new { MethodInfo = method, FirstParameter = method.GetParameters()[0] };

            foreach (var method in matchedMethods)
            {
                var methodCopy = method.MethodInfo;
                Type firstParameterType = methodCopy.GetParameters().First().ParameterType;
                var invokeAction = InvokeAction(methodCopy);
                eventHandlers.Add(firstParameterType, invokeAction);
            }
        }

        private Action<object> InvokeAction(MethodInfo methodCopy)
        {
            return e => methodCopy.Invoke(this, new[] { e });
        }
    }

    [DebuggerStepThrough]
    public class Entity : EntityBase
    {
        private IAggregate parent;

        protected Entity(IAggregate parent, IHaveIdentity identity)
            : base(identity)
        {
            SetParent(parent);
        }

        private void SetParent(IAggregate aggregate)
        {
            parent = aggregate;
            parent.RegisterOwnedEntity(this);
        }

        protected override void SaveEvent(IDomainEvent @event)
        {
            @event.EntityId = Identity;
            parent.SaveEvent(@event);
        }
    }

    public abstract class DomainEvent : IDomainEvent
    {
        public Guid EventId { get; protected set; }
        public DateTime OccurredAt { get; protected set; }
        public IHaveIdentity EntityId { get; set; }
        public IHaveIdentity AggregateId { get; set; }
        int IDomainEvent.Version { get; set; }

        protected DomainEvent()
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
        }
    }

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