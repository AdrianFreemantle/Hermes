namespace Hermes
{
    public interface IMessageBus
    {
        void Send(params object[] messages);
        void Send(Address address, params object[] messages);
    }
}
