using System;
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
        private readonly IProcessMessages messageProcessor;
        private readonly ITransportMessageFactory transportMessageFactory;

        public LocalBus(IDispatchMessagesToHandlers messageDispatcher, IProcessMessages messageProcessor, ITransportMessageFactory transportMessageFactory)
        {
            this.messageDispatcher = messageDispatcher;
            this.messageProcessor = messageProcessor;
            this.transportMessageFactory = transportMessageFactory;
        }

        public void Execute(Guid corrolationId, params object[] messages)
        {
            var transportMessage = transportMessageFactory.BuildTransportMessage(corrolationId, TimeSpan.MaxValue, messages);
            messageProcessor.ProcessTransportMessage(transportMessage);
        }

        public void Execute(params object[] messages)
        {
            var transportMessage = transportMessageFactory.BuildTransportMessage(messages);
            messageProcessor.ProcessTransportMessage(transportMessage);
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