using System;

using Hermes.Messaging.Bus.Transports;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging
{
    public interface IProcessIncomingMessages
    {
        void ProcessTransportMessage(TransportMessage transportMessage, IServiceLocator serviceLocator);
    }
}
