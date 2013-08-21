using System;
using System.Collections.Generic;

namespace Hermes.Core.Deferment
{
    public interface IPersistTimeouts
    {
        IEnumerable<long> GetExpired();
        void Add(TimeoutData timeout);
        bool TryRemove(long timeoutId, out TimeoutData timeoutData);
    }
}