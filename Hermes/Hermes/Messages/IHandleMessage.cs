namespace Hermes.Messages
{
    public interface IHandleMessage<in TMessage> where TMessage : IMessage
    {
        void Handle(TMessage command);
    }
}
