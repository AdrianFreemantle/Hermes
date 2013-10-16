using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public interface IInMemoryBus
    {
        void Execute(Guid corrolationId, params object[] messages);
        void Execute(params object[] commands);
        void Raise(params object[] events);
    }
}