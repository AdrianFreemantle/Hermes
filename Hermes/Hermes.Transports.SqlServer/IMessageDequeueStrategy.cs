using System;

namespace Hermes.Transports.SqlServer
{
    public interface IMessageDequeueStrategy
    {
        void Dequeue(Func<MessageEnvelope, bool> tryProcessMessage);
    }
}