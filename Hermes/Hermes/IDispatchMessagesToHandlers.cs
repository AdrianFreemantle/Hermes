using System.Collections.Generic;

using Microsoft.Practices.ServiceLocation;

namespace Hermes
{
    public interface IDispatchMessagesToHandlers
    {
        void DispatchToHandlers(IServiceLocator serviceLocator, object message);
    }
}