using System;
using System.Collections.Generic;

using Microsoft.Practices.ServiceLocation;

namespace Hermes
{
    public interface IBuildMessageHandlers
    {
        IEnumerable<Action> GetHandlers(IServiceLocator serviceLocator, object message);
    }
}