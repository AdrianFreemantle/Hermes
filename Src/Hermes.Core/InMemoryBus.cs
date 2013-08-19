using System;
using Microsoft.Practices.ServiceLocation;
using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace Hermes.Core
{
    public sealed class InMemoryBus : IInMemoryBus
    {
        private readonly IDispatchMessagesToHandlers messageDispatcher;

        public InMemoryBus(IDispatchMessagesToHandlers messageDispatcher)
        {
            this.messageDispatcher = messageDispatcher;
        }

        public void Raise(object @event)
        {
            messageDispatcher.DispatchToHandlers(ServiceLocator.Current.GetService<IServiceLocator>(), @event);
        }

        public void Execute(object command)
        {
            messageDispatcher.DispatchToHandlers(ServiceLocator.Current.GetService<IServiceLocator>(), command);
        }
    }
}