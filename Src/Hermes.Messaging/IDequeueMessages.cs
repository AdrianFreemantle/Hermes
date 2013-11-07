using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    public interface IDequeueMessages
    {
        TransportMessage Dequeue(Address address);
    }
}