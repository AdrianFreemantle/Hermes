namespace Hermes.Messaging
{
    public interface IInMemoryEventBus
    {
        void Raise(object @event);
    }
}