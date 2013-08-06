using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Hermes.Configuration;
using Hermes.Core.Deferment;
using Hermes.Serialization;
using Hermes.Transports;

namespace Hermes.Core
{
    public class MessageBus : IMessageBus, IStartableMessageBus, IInMemoryBus, IDisposable
    {
        private readonly ISerializeMessages messageSerializer;
        private readonly ITransportMessages messageTransport;
        private readonly IRouteMessageToEndpoint messageRouter;
        private readonly IPublishMessages messagePublisher;

        public IInMemoryBus InMemory { get { return this; } }

        public MessageBus(ISerializeMessages messageSerializer, ITransportMessages messageTransport, IRouteMessageToEndpoint messageRouter, IPublishMessages messagePublisher)
        {
            this.messageSerializer = messageSerializer;
            this.messageTransport = messageTransport;
            this.messageRouter = messageRouter;
            this.messagePublisher = messagePublisher;
        }

        public void Start()
        {
            messageTransport.Start(Settings.ThisEndpoint);
        }

        public void Stop()
        {
            messageTransport.Stop();
        }

        public void Dispose()
        {
            Stop();
        }

        public void Defer(TimeSpan delay, params object[] messages)
        {
            Defer(delay, Guid.Empty, messages);
        }

        public void Defer(TimeSpan delay, Guid correlationId, params object[] messages)
        {
            if (messages == null || messages.Length == 0)
            {
                return;
            }

            MessageEnvelope message = BuildMessageEnvelope(messages);
            message.Headers[TimeoutHeaders.Expire] = DateTime.UtcNow.Add(delay).ToWireFormattedString();
            message.Headers[TimeoutHeaders.RouteExpiredTimeoutTo] = messageRouter.GetDestinationFor(messages.First().GetType()).ToString();

            messageTransport.Send(message, Settings.DefermentEndpoint); 
        }        

        public void Send(params object[] messages)
        {
            if (messages == null || messages.Length == 0)
            {
                return;
            }

            Send(messageRouter.GetDestinationFor(messages.First().GetType()), messages);
        }

        public void Send(Address address, params object[] messages)
        {
            Send(address, Guid.Empty, messages);
        }

        public void Send(Address address, Guid corrolationId, params object[] messages)
        {
            if (messages == null || messages.Length == 0)
            {
                return;
            }

            MessageEnvelope message = BuildMessageEnvelope(messages, corrolationId);
            messageTransport.Send(message, address);
        }

        public void Publish(params object[] messages)
        {
            if (messages == null || messages.Length == 0) 
            {
                return;
            }

            var messageTypes = messages.Select(o => o.GetType());
            MessageEnvelope message = BuildMessageEnvelope(messages);
            messagePublisher.Publish(message, messageTypes);
        }

        private MessageEnvelope BuildMessageEnvelope(object[] messages)
        {
            return BuildMessageEnvelope(messages, Guid.Empty);
        }

        private MessageEnvelope BuildMessageEnvelope(object[] messages, Guid correlationId)
        {
            byte[] messageBody;

            using (var stream = new MemoryStream())
            {
                messageSerializer.Serialize(messages, stream);
                stream.Flush();
                messageBody = stream.ToArray();
            }

            var message = new MessageEnvelope(Guid.NewGuid(), correlationId, Address.Self, TimeSpan.MaxValue, true, new Dictionary<string, string>(), messageBody);

            return message;
        }        

        void IInMemoryBus.Raise(object @event)
        {
            var messageProcessor = Settings.Builder.GetInstance<IProcessMessages>();
            messageProcessor.DispatchToHandlers(new [] { @event });
        }
    }
}
