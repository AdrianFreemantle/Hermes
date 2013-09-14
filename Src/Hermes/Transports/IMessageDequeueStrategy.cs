using Hermes.Messaging;

namespace Hermes.Transports
{
    public interface IMessageDequeueStrategy
    {
        TransportMessage Dequeue(Address address);
    }
}