using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Clientele.Core.Domain
{
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
}