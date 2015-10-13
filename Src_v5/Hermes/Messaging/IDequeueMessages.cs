namespace Hermes.Messaging
{
    public interface IDequeueMessages
    {
        TransportMessage Dequeue();
    }
}