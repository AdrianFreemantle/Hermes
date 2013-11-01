using System;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging
{
    public interface IProcessIncomingMessages
    {
        void ProcessTransportMessage(TransportMessage transportMessage, IServiceLocator serviceLocator);
    }
}
