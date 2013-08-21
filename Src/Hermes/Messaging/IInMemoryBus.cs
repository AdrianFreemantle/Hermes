namespace Hermes.Messaging
{
    public interface IInMemoryBus
    {
        void Raise(params object[] @events);
        void Execute(params object[] commands);
    }
}