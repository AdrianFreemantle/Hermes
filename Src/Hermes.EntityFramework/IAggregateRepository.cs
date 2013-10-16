using Hermes.Domain;

namespace Hermes.EntityFramework
{
    public interface IAggregateRepository
    {
        TAggregate Get<TAggregate>(IIdentity id) where TAggregate : class, IAggregate;
        void Add(IAggregate aggregate);
        void Update(IAggregate aggregate);
    }
}