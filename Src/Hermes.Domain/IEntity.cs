namespace Hermes.Domain
{
    public interface IEntity : IAmRestorable
    {
        IIdentity Identity { get; }
        void ApplyEvent(DomainEvent @event);
    }
}