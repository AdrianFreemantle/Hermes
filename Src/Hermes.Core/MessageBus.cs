using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

using Hermes.Configuration;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Routing;
using Hermes.Serialization;
using Hermes.Transports;

namespace Hermes.Core
{
    public class MessageBus : IMessageBus, IStartableMessageBus, IDisposable
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof(MessageBus)); 

        private readonly ISerializeMessages messageSerializer;
        private readonly ITransportMessages messageTransport;
        private readonly IRouteMessageToEndpoint messageRouter;
        private readonly IPublishMessages messagePublisher;
        private readonly IProcessMessages messageProcessor;
        private readonly ThreadLocal<TransportMessage> currentMessageBeingProcessed = new ThreadLocal<TransportMessage>();        
        private readonly CallBackManager callBackManager = new CallBackManager();

        public IMessageContext CurrentMessageContext
        {
            get
            {
                return currentMessageBeingProcessed.Value == null
                    ? new MessageContext(TransportMessage.Undefined)
                    : new MessageContext(currentMessageBeingProcessed.Value);
            }
        }

        public MessageBus(ISerializeMessages messageSerializer, ITransportMessages messageTransport, IRouteMessageToEndpoint messageRouter, IPublishMessages messagePublisher, IProcessMessages messageProcessor)
        {
            this.messageSerializer = messageSerializer;
            this.messageTransport = messageTransport;
            this.messageRouter = messageRouter;
            this.messagePublisher = messagePublisher;
            this.messageProcessor = messageProcessor;
        }

        public void Start()
        {
            messageProcessor.CompletedMessageProcessing += CompletedMessageProcessing;
            messageProcessor.StartedMessageProcessing += StartedMessageProcessing;
            messageTransport.Start();
        }

        void StartedMessageProcessing(object sender, StartedMessageProcessingEventArgs e)
        {
            currentMessageBeingProcessed.Value = e.TransportMessage;
            callBackManager.HandleCorrelatedMessage(e.TransportMessage, e.Messages);
        }

        void CompletedMessageProcessing(object sender, CompletedMessageProcessingEventArgs e)
        {
            currentMessageBeingProcessed.Value = TransportMessage.Undefined;
        }

        public void Stop()
        {
            messageTransport.Stop();
            currentMessageBeingProcessed.Value = TransportMessage.Undefined;
            messageProcessor.CompletedMessageProcessing -= CompletedMessageProcessing;
            messageProcessor.StartedMessageProcessing -= StartedMessageProcessing;
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

            TransportMessage transportMessage = BuildTransportMessage(messages);
            transportMessage.Headers[Headers.TimeoutExpire] = DateTime.UtcNow.Add(delay).ToWireFormattedString();
            transportMessage.Headers[Headers.RouteExpiredTimeoutTo] = messageRouter.GetDestinationFor(messages.First().GetType()).ToString();

            messageTransport.Send(transportMessage, Settings.DefermentEndpoint); 
        }        

        public ICallback Send(params object[] messages)
        {
            Address destination = GetDestination(messages);
            return Send(destination, messages);
        }

        public ICallback Send(Address address, params object[] messages)
        {
            return Send(address, Guid.Empty, messages);
        }

        public ICallback Send(Address address, Guid corrolationId, params object[] messages)
        {
            return SendMessages(address, corrolationId, TimeSpan.MaxValue, messages);
        }

        public ICallback Send(Address address, Guid corrolationId, TimeSpan timeToLive, params object[] messages)
        {
            return SendMessages(address, corrolationId, timeToLive, messages);
        }

        public ICallback Send(Guid corrolationId, params object[] messages)
        {
            Address destination = GetDestination(messages);
            return SendMessages(destination, corrolationId, TimeSpan.MaxValue, messages);
        }

        public ICallback Send(Guid corrolationId, TimeSpan timeToLive, params object[] messages)
        {
            Address destination = GetDestination(messages);
            return SendMessages(destination, corrolationId, timeToLive, messages);
        }

        private ICallback SendMessages(Address address, Guid corrolationId, TimeSpan timeToLive, params object[] messages)
        {
            if (messages == null || messages.Length == 0)
                throw new InvalidOperationException("Cannot send an empty set of messages.");

            var transportMessage = BuildTransportMessage(corrolationId, timeToLive, messages);
            messageTransport.Send(transportMessage, address);

            return callBackManager.SetupCallback(transportMessage.CorrelationId);
        } 

        public void Reply(params object[] messages)
        {
            var currentMessage = currentMessageBeingProcessed.Value;

            if (currentMessage == null || currentMessage == TransportMessage.Undefined)
                throw new InvalidOperationException("Reply was called but we have no current message to reply to.");

            if (currentMessage.ReplyToAddress == Address.Undefined)
                throw new InvalidOperationException("Reply was called but the current message does not have a reply to address.");

            Send(currentMessage.ReplyToAddress, currentMessage.CorrelationId, messages);
        }

        public void Return<TEnum>(TEnum errorCode) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var currentMessage = currentMessageBeingProcessed.Value;

            if (currentMessage == null || currentMessage == TransportMessage.Undefined)
                throw new InvalidOperationException("Return was called but we have no current message to return.");

            if (currentMessage.ReplyToAddress == Address.Undefined)
                throw new InvalidOperationException("Return was called with undefined reply-to-address field.");

            var returnMessage = TransportMessage.BuildControlMessage(currentMessage.CorrelationId);
            returnMessage.Headers[Headers.ReturnMessageErrorCodeHeader] = errorCode.GetHashCode().ToString(CultureInfo.InvariantCulture);

            messageTransport.Send(returnMessage, currentMessage.ReplyToAddress);
        }

        private Address GetDestination(params object[] messages)
        {
            if (messages == null || messages.Length == 0)
                throw new InvalidOperationException("Cannot send an empty set of messages.");

            return messageRouter.GetDestinationFor(messages.First().GetType());
        }

        public void Publish(params object[] messages)
        {
            if (messages == null || messages.Length == 0) 
            {
                return;
            }

            var messageTypes = messages.Select(o => o.GetType());
            TransportMessage transportMessage = BuildTransportMessage(messages);
            messagePublisher.Publish(transportMessage, messageTypes);
        }

        private TransportMessage BuildTransportMessage(object[] messages)
        {
            return BuildTransportMessage(Guid.Empty, TimeSpan.MaxValue, messages);
        }

        private TransportMessage BuildTransportMessage(Guid correlationId, TimeSpan timeToLive, object[] messages)
        {
            byte[] messageBody;

            using (var stream = new MemoryStream())
            {
                messageSerializer.Serialize(messages, stream);
                stream.Flush();
                messageBody = stream.ToArray();
            }

            var message = new TransportMessage(IdentityFactory.NewComb(), correlationId, Address.Local, timeToLive, new Dictionary<string, string>(), messageBody);

            return message;
        }        
    }
}
