namespace Hermes
{
    public interface IStartableMessageBus
    {
        void Start(Address localAddress);
        void Stop();
    }
}