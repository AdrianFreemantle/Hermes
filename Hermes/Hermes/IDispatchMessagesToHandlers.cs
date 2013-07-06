using System.Collections.Generic;

namespace Hermes
{
    public interface IDispatchMessagesToHandlers
    {
        void DispatchToHandlers(object message);
    }
}