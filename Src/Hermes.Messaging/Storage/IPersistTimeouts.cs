using System;
using System.Collections.Generic;

namespace Hermes.Messaging.Storage
{
    public interface IPersistTimeouts
    {
        void Add(TimeoutData timeout);
        void Add(Guid correlationId, TimeSpan timeToLive, object[] messages, IDictionary<string, string> headers);
        bool TryFetchNextTimeout(out TimeoutData timeoutData);
    }
}