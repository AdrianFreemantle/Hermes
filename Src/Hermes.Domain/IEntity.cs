namespace Hermes.Domain
{
    public interface IEntity : IAmRestorable
    {
        IIdentity Identity { get; }
        bool ApplyEvent(IAggregateEvent @event);
    }
}