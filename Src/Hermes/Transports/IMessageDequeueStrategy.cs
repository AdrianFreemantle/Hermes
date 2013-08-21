using Hermes.Messaging;

namespace Hermes.Transports
{
    public interface IMessageDequeueStrategy
    {
        MessageEnvelope Dequeue(Address address);
    }
}