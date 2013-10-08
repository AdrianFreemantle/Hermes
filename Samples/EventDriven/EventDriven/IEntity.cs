namespace EventDriven
{
    public interface IEntity : IAmRestorable
    {
        IHaveIdentity Identity { get; }
        void ApplyEvent(DomainEvent @event);
    }
}