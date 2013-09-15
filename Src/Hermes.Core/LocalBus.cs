using System.Collections.Generic;

using Hermes.Logging;
using Hermes.Messaging;

using Microsoft.Practices.ServiceLocation;

using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace Hermes.Core
{
    public class LocalBus : IInMemoryBus
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof (MessageBus));
        readonly IDispatchMessagesToHandlers messageDispatcher;

        public LocalBus(IDispatchMessagesToHandlers messageDispatcher)
        {
            this.messageDispatcher = messageDispatcher;
        }

        void IInMemoryBus.Raise(params object[] events)
        {
            var serviceLocator = ServiceLocator.Current.GetService<IServiceLocator>();
            Raise(events, serviceLocator);
        }

        private void Raise(IEnumerable<object> events, IServiceLocator serviceLocator)
        {
            foreach (var @event in events)
            {
                messageDispatcher.DispatchToHandlers(serviceLocator, @event);
            }
        }
    }
}