using System;

namespace Hermes.Messaging
{
    public interface IPersistTimeouts
    {
        void Add(MessageContext timeout);
        void Purge();
        bool TryFetchNextTimeout(out MessageContext timeoutData);
        void Remove(Guid correlationId);
    }
}
