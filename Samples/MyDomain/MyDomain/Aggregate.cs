using System;
using System.Collections.Generic;

namespace MyDomain
{
    public abstract class Aggregate : IAggregate
    {
        public int Version { get; private set; }
        public Guid Id { get; protected set; }
        
        private readonly HashSet<object> uncommittedEvents = new HashSet<object>();

        void IAggregate.ClearUncommittedEvents()
        {
            uncommittedEvents.Clear();
        }

        IEnumerable<object> IAggregate.GetUncommittedEvents()
        {
            return uncommittedEvents;
        }

        void IAggregate.LoadFromHistory(IEnumerable<object> domainEvents)
        {
            if (Version != 0)
            {
                throw new Exception("Version");
            }

            foreach (var @event in domainEvents)
            {
                ApplyEvent(@event);
                Version++;
            }
        }

        protected void RaiseEvent(IEnumerable<object> events)
        {
            foreach (var @event in events)
            {
                RaiseEvent(@event);
            }
        }

        protected void RaiseEvent(object @event)
        {
            ApplyEvent(@event);
            SaveChange(@event);
        }

        internal void SaveChange(object @event)
        {
            Version++;
            uncommittedEvents.Add(@event);
        }

        void IAggregate.ApplyEvent(object @event)
        {
            ApplyEvent(@event);
        }

        protected abstract void ApplyEvent(object @event);

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Aggregate);
        }

        public virtual bool Equals(Aggregate other)
        {
            if (null != other && other.GetType() == GetType())
            {
                return other.Id.Equals(Id);
            }

            return false;
        }

        public static bool operator ==(Aggregate left, Aggregate right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Aggregate left, Aggregate right)
        {
            return !Equals(left, right);
        }

        IMemento IAggregate.GetSnapshot()
        {
            return GetSnapshot();
        }

        void IAggregate.RestoreSnapshot(IMemento memento)
        {
            if (memento == null)
            {
                return;
            }

            RestoreSnapshot(memento);
        }

        protected virtual IMemento GetSnapshot()
        {
            return null;
        }

        protected virtual void RestoreSnapshot(IMemento memento)
        {
            throw new NotImplementedException("The entity does not currently support restoring from a snapshot");
        }
    }
}