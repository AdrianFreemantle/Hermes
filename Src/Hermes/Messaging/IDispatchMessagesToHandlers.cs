using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging
{
    public interface IDispatchMessagesToHandlers
    {
        void DispatchToHandlers(IServiceLocator serviceLocator, object message);
    }
}