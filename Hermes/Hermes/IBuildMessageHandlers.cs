using System;
using System.Collections.Generic;

namespace Hermes
{
    public interface IBuildMessageHandlers
    {
        IEnumerable<object> GetMessageHandlers(Type messageType);
    }
}