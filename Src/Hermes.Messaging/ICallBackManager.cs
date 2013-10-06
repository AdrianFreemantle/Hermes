using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public interface ICallBackManager
    {
        void HandleCallback(TransportMessage message, IReadOnlyCollection<object> messages);
        ICallback SetupCallback(Guid messageId);
    }
}
