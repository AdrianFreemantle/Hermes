using System;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    public interface IManageCallbacks
    {
        void HandleCallback(IncomingMessageContext context);
        ICallback SetupCallback(Guid messageId);
    }
}
