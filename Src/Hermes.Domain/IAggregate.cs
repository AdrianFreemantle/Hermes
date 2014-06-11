using System.Collections.Generic;

namespace Hermes.Domain
{
    public interface IAggregate : IEntity
    {
        IEnumerable<IAggregateEvent> GetUncommittedEvents();
        void ClearUncommittedEvents();
        int GetVersion();
        void LoadFromHistory(IEnumerable<IAggregateEvent> domainEvents);
    }
}