using System;
using System.Threading;

using Hermes.Messaging;

using Microsoft.Practices.ServiceLocation;

using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace MyDomain.Shell
{
    public sealed class DomainEvent
    {
        private static readonly ThreadLocal<DomainEvent> instance = new ThreadLocal<DomainEvent>();
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
                if (!instance.IsValueCreated)
                {
                    lock (SyncRoot)
                    {
                        if (!instance.IsValueCreated)
                        {
                            instance.Value = new DomainEvent();
                        }
                    }
                }

                return instance.Value;
            }
        }

        public void Raise(object @event)
        {
            messageDispatcher.DispatchToHandlers(ServiceLocator.Current.GetService<IServiceLocator>(), @event);
        }
    }
}