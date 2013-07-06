using System;

namespace Hermes.Transports.SqlServer
{
    public interface IMessageDequeueStrategy
    {
        MessageEnvelope Dequeue(Address address);
    }
}