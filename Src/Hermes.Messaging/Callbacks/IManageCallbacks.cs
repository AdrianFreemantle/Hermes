using System;
using Hermes.Messaging.Pipeline;

namespace Hermes.Messaging.Callbacks
{
    public interface IManageCallbacks
    {
        void HandleCallback(IncomingMessageContext context);
        ICallback SetupCallback(Guid messageId);
    }
}
