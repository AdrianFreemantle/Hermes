﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hermes.Domain
{
    public abstract class EntityBase : IEntity
    {
        public IIdentity Identity { get; set; }
        private readonly List<EventHandlerProperties> eventHandlers;

        protected EntityBase(IIdentity identity)
        {
            Identity = identity;
            eventHandlers = new List<EventHandlerProperties>();
            GetEventHandlers();
        }

        internal protected void RaiseEvent(IDomainEvent @event)  
        {
            if (ApplyEvent(@event, false))
            {
                SaveEvent(@event, this);
                return;
            }

            throw new EventHandlerNotFoundException(this, @event);
        }

        bool IEntity.ApplyEvent(IDomainEvent @event)
        {
            return ApplyEvent(@event, true);
        }

        internal protected virtual bool ApplyEvent(IDomainEvent @event, bool isReplay)
        {
            try
            {
                EventHandlerProperties handler = isReplay 
                    ? eventHandlers.First(h => h.EventIsOwnedByEntity(@event, this))
                    : eventHandlers.First(h => h.CanHandleEvent(@event));

                if (handler != null)
                {
                    handler.InvokeHandler(@event, this);
                    return true;
                }
            }
            catch (TargetInvocationException ex)
            {
                throw new EventHandlerInvocationException(this, @event, ex);
            }

            return false;
        }

        internal protected abstract void SaveEvent(IDomainEvent @event, EntityBase source);


        internal void UpdateEventDetails(IDomainEvent @event, IAggregate aggregate)
        {
            var handler = eventHandlers.First(h => h.CanHandleEvent(@event));
            handler.UpdateEventDetails(@event, aggregate, this);
        }

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

        //[DebuggerStepThrough]
        private void GetEventHandlers()
        {
            Type eventBaseType = typeof(IDomainEvent);

            var targetType = GetType();

            var methodsToMatch = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var matchedMethods = from method in methodsToMatch
                                 let parameters = method.GetParameters()
                                 where
                                     method.Name.Equals("When", StringComparison.InvariantCulture) &&
                                         parameters.Length == 1 &&
                                         eventBaseType.IsAssignableFrom(parameters[0].ParameterType)
                                 select
                                     new { MethodInfo = method, FirstParameter = method.GetParameters()[0] };

            foreach (var method in matchedMethods)
            {
                MethodInfo methodCopy = method.MethodInfo;
                Type eventType = methodCopy.GetParameters().First().ParameterType;
                eventHandlers.Add(EventHandlerProperties.CreateFromMethodInfo(methodCopy, targetType));
            }
        }
    }
}