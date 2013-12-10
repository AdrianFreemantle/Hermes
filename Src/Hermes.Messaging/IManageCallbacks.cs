using System;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    public interface IManageCallbacks
    {
        void HandleCallback(IncomingMessageContext context);
        ICallback SetupCallback(Guid messageId);
    }
}
