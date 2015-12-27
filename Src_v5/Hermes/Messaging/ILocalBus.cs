namespace Hermes.Messaging
{
    public interface ILocalBus 
    {
        void Execute(IDomainCommand command);
        void Raise(IDomainEvent @event);
    }
}