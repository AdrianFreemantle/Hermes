using Hermes.Messaging.Bus.Transports;

namespace Hermes.Messaging
{
    public interface IDequeueMessages
    {
        TransportMessage Dequeue(Address address);
    }
}