using System;
using System.Collections.Generic;

namespace MyDomain
{
    public interface IAggregate
    {
        int Version { get; }
        Guid Identity { get; }
        IEnumerable<object> GetUncommittedEvents();
        void ClearUncommittedEvents();
        void ApplyEvent(object @event);
        void LoadFromHistory(IEnumerable<object> domainEvents);
        void RestoreSnapshot(IMemento memento);
        IMemento GetSnapshot();
    }
}