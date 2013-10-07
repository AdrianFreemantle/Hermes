using System;

using Hermes.Ioc;
using Hermes.Logging;

namespace Hermes.Messaging
{
    public class LocalBus : IInMemoryBus
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof (MessageBus));
        private readonly IProcessIncomingMessages incomingMessageProcessor;
        private readonly ITransportMessageFactory transportMessageFactory;

        public LocalBus(IProcessIncomingMessages incomingMessageProcessor, ITransportMessageFactory transportMessageFactory)
        {
            this.incomingMessageProcessor = incomingMessageProcessor;
            this.transportMessageFactory = transportMessageFactory;
            this.transportMessageFactory = transportMessageFactory;
        }

        public void Execute(Guid corrolationId, params ICommand[] messages)
        {
            var transportMessage = transportMessageFactory.BuildTransportMessage(corrolationId, TimeSpan.MaxValue, messages);
            incomingMessageProcessor.ProcessTransportMessage(transportMessage);
        }

        public void Execute(params ICommand[] messages)
        {
            var transportMessage = transportMessageFactory.BuildTransportMessage(messages);
            incomingMessageProcessor.ProcessTransportMessage(transportMessage);
        }

        void IInMemoryBus.Raise(params IEvent[] events)
        {
            var dispatcher = ServiceLocator.Current.GetService<IDispatchMessagesToHandlers>();

            foreach (var @event in events)
            {
                dispatcher.DispatchToHandlers(@event);
            }
        }
    }
}