using System;
using System.Collections.Generic;

using Hermes.Messaging.Timeouts;

namespace Hermes.Messaging
{
    public interface IPersistTimeouts
    {
        void Add(TimeoutData timeout);
        void Purge();
        //void Add(Guid correlationId, TimeSpan timeToLive, object[] messages, IDictionary<string, string> headers);
        bool TryFetchNextTimeout(out TimeoutData timeoutData);
    }
}