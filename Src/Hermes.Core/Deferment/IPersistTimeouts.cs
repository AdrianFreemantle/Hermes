using System;
using System.Collections.Generic;

namespace Hermes.Core.Deferment
{
    public interface IPersistTimeouts
    {
        void Add(TimeoutData timeout);
        bool TryFetchNextTimeout(out TimeoutData timeoutData);
    }
}