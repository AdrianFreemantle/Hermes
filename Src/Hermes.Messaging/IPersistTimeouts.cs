using System;
using System.Collections.Generic;

using Hermes.Messaging.Timeouts;

namespace Hermes.Messaging
{
    public interface IPersistTimeouts
    {
        void Add(TimeoutData timeout);
        void Purge();
        bool TryFetchNextTimeout(out TimeoutData timeoutData);
        void Remove(Guid correlationId);
    }
}