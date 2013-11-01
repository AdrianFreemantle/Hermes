using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging
{
    public interface IDispatchMessagesToHandlers
    {
        void DispatchToHandlers(object message, IServiceLocator serviceLocator);
    }
}