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

        public void Defer(TimeSpan delay, object command)
        {
            Defer(delay, Guid.Empty, new[] { command });
        }

        public void Defer(TimeSpan delay, ICollection<object> commands)
        {
            Defer(delay, Guid.Empty, commands);
        }

        public void Defer(TimeSpan delay, Guid correlationId, object commands)
        {
            Defer(delay, Guid.Empty, new[] { commands });
        }

        public void Defer(TimeSpan delay, Guid correlationId, ICollection<object> commands)
        {
            if (commands == null || !commands.Any())
            {
                return;
            }

            MessageEnvelope message = BuildMessageEnvelope(commands);
            message.Headers[Headers.TimeoutExpire] = DateTime.UtcNow.Add(delay).ToWireFormattedString();
            message.Headers[Headers.RouteExpiredTimeoutTo] = messageRouter.GetDestinationFor(commands.First().GetType()).ToString();

            messageTransport.Send(message, Settings.DefermentEndpoint); 
        }

        public void Send(object command)
        {
            Send(new[] { command });
        }

        public void Send(ICollection<object> commands)
        {
            if (commands == null || !commands.Any())
            {
                return;
            }

            Address destination = messageRouter.GetDestinationFor(commands.First().GetType());

            Send(destination, commands);
        }

        public void Send(Address address, object command)
        {
            Send(address, Guid.Empty, new[] { command });
        }

        public void Send(Address address, ICollection<object> commands)
        {
            Send(address, Guid.Empty, commands);
        }

        public void Send(Address address, Guid corrolationId, object command)
        {
            Send(address, corrolationId, new[] { command });
        }

        public void Send(Address address, Guid corrolationId, ICollection<object> commands)
        {
            if (commands == null || !commands.Any())
            {
                return;
            }

            MessageEnvelope message = BuildMessageEnvelope(commands, corrolationId);
            messageTransport.Send(message, address);
        }

        public void Publish(object @event)
        {
            Publish(new [] { @event });
        }

        public void Publish(ICollection<object> events)
        {
            if (events == null || !events.Any()) 
            {
                return;
            }

            var messageTypes = events.Select(o => o.GetType());
            MessageEnvelope message = BuildMessageEnvelope(events);
            messagePublisher.Publish(message, messageTypes);
        }

        private MessageEnvelope BuildMessageEnvelope(IEnumerable<object> messages)
        {
            return BuildMessageEnvelope(messages, Guid.Empty);
        }

        private MessageEnvelope BuildMessageEnvelope(IEnumerable<object> messages, Guid correlationId)
        {
            byte[] messageBody;

            using (var stream = new MemoryStream())
            {
                messageSerializer.Serialize(messages.ToArray(), stream);
                stream.Flush();
                messageBody = stream.ToArray();
            }

            var message = new MessageEnvelope(Guid.NewGuid(), correlationId, TimeSpan.MaxValue, true, new Dictionary<string, string>(), messageBody);

            return message;
        }

        void IInMemoryBus.Raise(object @event)
        {
            Raise(new [] { @event });
        }

        void IInMemoryBus.Raise(ICollection<object> events)
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

        void IInMemoryBus.Execute(object command)
        {
            Execute(new[] { command });
        }

        void IInMemoryBus.Execute(ICollection<object> commands)
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
