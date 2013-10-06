using System;

using Hermes.Ioc;
using Hermes.Logging;

namespace Hermes.Messaging
{
    public class LocalBus : IInMemoryBus
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof (MessageBus));
        private readonly IProcessIncommingMessages incommingMessageProcessor;
        private readonly ITransportMessageFactory transportMessageFactory;

        public LocalBus(IProcessIncommingMessages incommingMessageProcessor, ITransportMessageFactory transportMessageFactory)
        {
            this.incommingMessageProcessor = incommingMessageProcessor;
            this.transportMessageFactory = transportMessageFactory;
            this.transportMessageFactory = transportMessageFactory;
        }

        public void Execute(Guid corrolationId, params ICommand[] messages)
        {
            var transportMessage = transportMessageFactory.BuildTransportMessage(corrolationId, TimeSpan.MaxValue, messages);
            incommingMessageProcessor.ProcessTransportMessage(transportMessage);
        }

        public void Execute(params ICommand[] messages)
        {
            var transportMessage = transportMessageFactory.BuildTransportMessage(messages);
            incommingMessageProcessor.ProcessTransportMessage(transportMessage);
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