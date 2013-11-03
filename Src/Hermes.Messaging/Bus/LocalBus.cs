using System;

using Hermes.Ioc;
using Hermes.Messaging.Bus.Transports;

namespace Hermes.Messaging.Bus
{
    public class LocalBus : IInMemoryBus
    {
        private readonly ITransportMessages messageTransport;
        private readonly ITransportMessageFactory transportMessageFactory;

        public LocalBus(ITransportMessages messageTransport, ITransportMessageFactory transportMessageFactory)
        {
            this.messageTransport = messageTransport;
            this.transportMessageFactory = transportMessageFactory;
            this.transportMessageFactory = transportMessageFactory;
        }

        public void Execute(Guid corrolationId, params object[] messages)
        {
            if (messageTransport.CurrentTransportMessage != TransportMessage.Undefined)
            {
                throw new InvalidOperationException("Only one comand may be processed at a time. Either group all required commands together in a single Execute call or send additional commands via the message bus.");
            }

            MessageRuleValidation.ValidateIsCommandType(messages);
            var transportMessage = transportMessageFactory.BuildTransportMessage(corrolationId, TimeSpan.MaxValue, messages);
            messageTransport.OnMessageReceived(transportMessage);
        }

        public void Execute(params object[] messages)
        {
            MessageRuleValidation.ValidateIsCommandType(messages);
            Execute(Guid.Empty, messages);
        }

        void IInMemoryBus.Raise(params object[] events)
        {
            MessageRuleValidation.ValidateIsEventType(events);
            var dispatcher = ServiceLocator.Current.GetService<IDispatchMessagesToHandlers>();

            foreach (var @event in events)
            {
                dispatcher.DispatchToHandlers(@event, ServiceLocator.Current);
            }
        }
    }
}