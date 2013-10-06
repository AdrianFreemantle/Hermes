namespace Hermes.Messaging.Transports
{
    public interface IMessageDequeueStrategy
    {
        TransportMessage Dequeue(Address address);
    }
}