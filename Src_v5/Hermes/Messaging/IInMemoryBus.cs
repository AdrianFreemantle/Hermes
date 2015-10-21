namespace Hermes.Messaging
{
    public interface IInMemoryBus 
    {
        void Execute(IDomainCommand command);
        void Raise(IDomainEvent @event);
    }
}