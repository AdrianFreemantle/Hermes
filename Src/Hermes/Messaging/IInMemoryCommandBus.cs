namespace Hermes.Messaging
{
    public interface IInMemoryCommandBus
    {
        void Execute(object command);
    }
}
