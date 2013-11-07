using Hermes.Messaging.Transports;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging
{
    public interface IIncomingMessageContext : IMessageContext
    {
        void Process(TransportMessage incomingMessage, IServiceLocator serviceLocator);
    }
}