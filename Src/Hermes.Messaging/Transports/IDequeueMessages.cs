namespace Hermes.Messaging.Transports
{
    public interface IDequeueMessages
    {
        TransportMessage Dequeue(Address address);
    }
}