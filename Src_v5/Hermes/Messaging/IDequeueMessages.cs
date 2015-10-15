namespace Hermes.Messaging
{
    public interface IDequeueMessages
    {
        IMessageContext Dequeue();
    }
}