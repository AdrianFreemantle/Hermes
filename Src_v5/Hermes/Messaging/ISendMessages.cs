namespace Hermes.Messaging
{
    public interface ISendMessages
    {
        void Send(IMessageContext message, Address address);
    }
}