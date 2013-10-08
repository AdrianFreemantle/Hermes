using System;

namespace Hermes.Messaging
{
    public interface IManageCallbacks
    {
        void HandleCallback(TransportMessage message, object[] messages);
        ICallback SetupCallback(Guid messageId);
    }
}
