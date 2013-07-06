using System;

namespace Hermes.Transports.SqlServer
{
    public interface IMessageDequeueStrategy
    {
        void Dequeue(Address address, Func<MessageEnvelope, bool> processMessage);
    }
}