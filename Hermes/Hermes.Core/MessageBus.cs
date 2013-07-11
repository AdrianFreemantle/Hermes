using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Hermes.Serialization;
using Hermes.Subscriptions;
using Hermes.Transports;

namespace Hermes.Core
{
    public class MessageBus : IMessageBus, IStartableMessageBus, IDisposable
    {
        private readonly ISerializeMessages messageSerializer;
        private readonly ITransportMessages messageTransport;
        private readonly IRouteMessageToEndpoint messageRouter;
        private readonly IPublishMessages messagePublisher;
        private readonly IStoreSubscriptions subscriptionStore;

        public MessageBus(ISerializeMessages messageSerializer, ITransportMessages messageTransport, IRouteMessageToEndpoint messageRouter, IPublishMessages messagePublisher, IStoreSubscriptions subscriptionStore)
        {
            this.messageSerializer = messageSerializer;
            this.messageTransport = messageTransport;
            this.messageRouter = messageRouter;
            this.messagePublisher = messagePublisher;
            this.subscriptionStore = subscriptionStore;
        }

        public void Start(Address localAddress)
        {
            messageTransport.Start(localAddress);
        }

        public void Stop()
        {
            messageTransport.Stop();
        }

        public void Dispose()
        {
            Stop();
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
            MessageEnvelope message = BuildMessageEnvelope(messages);
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
            byte[] messageBody;

            using (var stream = new MemoryStream())
            {
                messageSerializer.Serialize(messages, stream);
                stream.Flush();
                messageBody = stream.ToArray();
            }

            string debug = Encoding.UTF8.GetString(messageBody);

            var l = debug.Length;

            var message = new MessageEnvelope(Guid.NewGuid(), Guid.Empty, Address.Self, TimeSpan.MaxValue, true, new Dictionary<string, string>(), messageBody);

            return message;
        }

        public void Subscribe<T>()
        {
            Subscribe(typeof(T));
        }

        public void Subscribe(Type messageType)
        {
            var destination = messageRouter.GetDestinationFor(messageType);
            subscriptionStore.Subscribe(destination, messageType);
        }

        public void Unsubscribe<T>()
        {
            Unsubscribe(typeof(T));
        }

        public void Unsubscribe(Type messageType)
        {
            var destination = messageRouter.GetDestinationFor(messageType);
            subscriptionStore.Unsubscribe(destination, messageType);
        }
    }
}
