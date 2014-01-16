using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public interface IInMemoryBus
    {
        void Execute(object command);
        void Raise(object @event);
    }
}