namespace Hermes.Messaging
{
    public interface IHandleMessage<in TMessage> where TMessage : IMessage
    {
        void Handle(TMessage message);
    }
}
