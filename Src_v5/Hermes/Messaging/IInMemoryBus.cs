namespace Hermes.Messaging
{
    public interface IInMemoryBus 
    {
        void Execute(object command);
        void Raise(object @event);
    }
}