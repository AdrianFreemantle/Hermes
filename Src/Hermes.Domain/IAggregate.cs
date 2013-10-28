using System.Collections.Generic;

namespace Hermes.Domain
{
    public interface IAggregate : IEntity
    {
        IEnumerable<IDomainEvent> GetUncommittedEvents();
        void ClearUncommittedEvents();
        int GetVersion();
        void LoadFromHistory(IEnumerable<IDomainEvent> domainEvents);
    }
}