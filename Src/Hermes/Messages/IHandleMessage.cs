namespace Hermes.Messages
{
    public interface IHandleMessage<in TMessage> 
    {
        void Handle(TMessage message);
    }
}
