namespace Hermes
{
    public interface IInMemoryBus
    {
        void Raise(params object[] @events);
        void Execute(params object[] command);
    }
}