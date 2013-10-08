using System.Collections.Generic;

namespace EventDriven
{
    public interface IAggregate : IEntity
    {
        IEnumerable<DomainEvent> GetUncommittedEvents();
        void ClearUncommittedEvents();
        int GetVersion();
        void LoadFromHistory(IEnumerable<DomainEvent> domainEvents);
    }
}