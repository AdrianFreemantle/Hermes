using System;

namespace Hermes.Messaging
{
    public interface IProcessIncomingMessages
    {
        void ProcessTransportMessage(TransportMessage transportMessage);
    }
}
