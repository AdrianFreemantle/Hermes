namespace Hermes.Messaging
{
    public interface IInMemoryCommandBus
    {
        void Execute<T>(T command) where T : class;
    }
}
