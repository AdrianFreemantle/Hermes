using System;
using Microsoft.Practices.ServiceLocation;

using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace Hermes.Messages
{
    public sealed class DomainEvent
    {
        [ThreadStatic]
        private static volatile DomainEvent instance;
        private static readonly object SyncRoot = new Object();

        private readonly IDispatchMessagesToHandlers messageDispatcher;

        private DomainEvent()
        {
            messageDispatcher = ServiceLocator.Current.GetService<IDispatchMessagesToHandlers>();
        }

        public static DomainEvent Current
        {
            get
            {
                if (instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (instance == null)
                            instance = new DomainEvent();
                    }
                }

                return instance;
            }
        }

        public void Raise(object @event)
        {
            messageDispatcher.DispatchToHandlers(ServiceLocator.Current.GetService<IServiceLocator>(), @event);
        }
    }
}
