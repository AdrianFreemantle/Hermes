using System;
using System.Collections.Generic;

namespace Hermes.Core.Deferment
{
    public interface IPersistTimeouts
    {
        IEnumerable<Guid> GetExpired();
        void Add(TimeoutData timeout);
        bool TryRemove(Guid timeoutId, out TimeoutData timeoutData);
    }
}