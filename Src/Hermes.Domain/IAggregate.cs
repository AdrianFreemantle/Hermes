using System.Collections.Generic;

namespace Hermes.Domain
{
    public interface IAggregate : IEntity
    {
        IEnumerable<DomainEvent> GetUncommittedEvents();
        void ClearUncommittedEvents();
        int GetVersion();
        void LoadFromHistory(IEnumerable<DomainEvent> domainEvents);
    }
}