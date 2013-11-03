using System;

using Hermes.Messaging.Bus.Transports;

namespace Hermes.Messaging
{
    public interface IManageCallbacks
    {
        void HandleCallback(TransportMessage message, object[] messages);
        ICallback SetupCallback(Guid messageId);
    }
}
