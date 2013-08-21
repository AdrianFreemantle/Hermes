using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;

using Hermes.Configuration;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Serialization;
using Hermes.Transports;

namespace Hermes.Core
{
    public class Bus : IMessageBus, IInMemoryBus, IStartableMessageBus, IDisposable
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof(Bus)); 

        private readonly ISerializeMessages messageSerializer;
        private readonly ITransportMessages messageTransport;
        private readonly IRouteMessageToEndpoint messageRouter;
        private readonly IPublishMessages messagePublisher;
        private readonly IProcessMessages messageProcessor;

        public IInMemoryBus InMemory { get { return this; } }

        public Bus(ISerializeMessages messageSerializer, ITransportMessages messageTransport, IRouteMessageToEndpoint messageRouter, IPublishMessages messagePublisher, IProcessMessages messageProcessor)
        {
            this.messageSerializer = messageSerializer;
            this.messageTransport = messageTransport;
            this.messageRouter = messageRouter;
            this.messagePublisher = messagePublisher;
            this.messageProcessor = messageProcessor;
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
            message.Headers[Headers.TimeoutExpire] = DateTime.UtcNow.Add(delay).ToWireFormattedString();
            message.Headers[Headers.RouteExpiredTimeoutTo] = messageRouter.GetDestinationFor(messages.First().GetType()).ToString();

            messageTransport.Send(message, Settings.DefermentEndpoint); 
        }

        public void Send(params object[] messages)
        {
            if (messages == null || messages.Length == 0)
            {
                return;
            }

            Address destination = messageRouter.GetDestinationFor(messages.First().GetType());

            Send(destination, messages);
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

            var message = new MessageEnvelope(Guid.NewGuid(), correlationId, TimeSpan.MaxValue, true, new Dictionary<string, string>(), messageBody);

            return message;
        }

        void IInMemoryBus.Raise(params object[] events)
        {
            Retry.Action(() => Raise(events), OnRetryError, Settings.FirstLevelRetryAttempts, Settings.FirstLevelRetryDelay);
        }

        private void Raise(IEnumerable<object> events)
        {
            using (var scope = TransactionScopeUtils.Begin(TransactionScopeOption.RequiresNew))
            {
                messageProcessor.ProcessMessages(events);
                scope.Complete();
            }
        }

        void IInMemoryBus.Execute(params object[] commands)
        {
            Retry.Action(() => Execute(commands), OnRetryError, Settings.FirstLevelRetryAttempts, Settings.FirstLevelRetryDelay);
        }

        private void Execute(IEnumerable<object> commands)
        {
            using (var scope = TransactionScopeUtils.Begin(TransactionScopeOption.RequiresNew))
            {
                messageProcessor.ProcessMessages(commands);
                scope.Complete();
            }
        }

        private void OnRetryError(Exception ex)
        {
            logger.Warn("Error while processing in memory message, attempting retry : {0}", ex.GetFullExceptionMessage());
        } 
    }
}
