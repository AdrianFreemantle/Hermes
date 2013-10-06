using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public interface IInMemoryBus
    {
        void Execute(Guid corrolationId, params ICommand[] messages);
        void Execute(params ICommand[] commands);
        void Raise(params IEvent[] events);
    }
}