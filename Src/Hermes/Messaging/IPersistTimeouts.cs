using System;
using Hermes.Messaging.Timeouts;

namespace Hermes.Messaging
{
    public interface IPersistTimeouts
    {
        void Add(ITimeoutData timeout);
        void Purge();
        bool TryFetchNextTimeout(out ITimeoutData timeoutData);
        void Remove(Guid correlationId);
    }
}