using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging.Bus.Transports;
using Hermes.Messaging.Configuration;

namespace Hermes.Messaging.Bus
{
    public class MessageBus : IMessageBus, IAmStartable, IDisposable
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof(MessageBus)); 

        private readonly ITransportMessages messageTransport;
        private readonly IRouteMessageToEndpoint messageRouter;
        private readonly IPublishMessages messagePublisher;
        private readonly IPersistTimeouts timeoutPersistence;

        public IMessageContext CurrentMessageContext
        {
            get { return new MessageContext(messageTransport.CurrentTransportMessage); }
        }

        public MessageBus(ITransportMessages messageTransport, IRouteMessageToEndpoint messageRouter, IPublishMessages messagePublisher)
        {
            this.messageTransport = messageTransport;
            this.messageRouter = messageRouter;
            this.messagePublisher = messagePublisher;
        }

        public void Start()
        {            
            messageTransport.Start();
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
            if (Settings.IsClientEndpoint)
            {
                throw new NotSupportedException("Client endpoints may not defer messages.");
            }

            if (messages == null || messages.Length == 0)
                throw new InvalidOperationException("Cannot send an empty set of messages.");

            string destination = messageRouter.GetDestinationFor(messages.First().GetType()).ToString();
            string timeout = DateTime.UtcNow.Add(delay).ToWireFormattedString();

            var headers = new Dictionary<string, string>
            {
                {HeaderKeys.TimeoutExpire, timeout},
                {HeaderKeys.RouteExpiredTimeoutTo, destination}
            };

            MessageRuleValidation.ValidateIsCommandType(messages);
            timeoutPersistence.Add(correlationId, delay, messages, headers); 
        }

        public ICallback Send(params object[] messages)
        {
            Address destination = messageRouter.GetDestinationFor(messages.First().GetType());
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
            Address destination = messageRouter.GetDestinationFor(messages.First().GetType());
            return SendMessages(destination, corrolationId, TimeSpan.MaxValue, messages);
        }

        public ICallback Send(Guid corrolationId, TimeSpan timeToLive, params object[] messages)
        {
            Address destination = messageRouter.GetDestinationFor(messages.First().GetType());
            return SendMessages(destination, corrolationId, timeToLive, messages);
        }

        private ICallback SendMessages(Address address, Guid corrolationId, TimeSpan timeToLive, params object[] messages)
        {
            MessageRuleValidation.ValidateIsCommandType(messages);
            return messageTransport.SendMessage(address, corrolationId, timeToLive, messages);
        }

        public void Reply(params object[] messages)
        {
            var currentMessage = messageTransport.CurrentTransportMessage;

            if (currentMessage == null || currentMessage == TransportMessage.Undefined)
                throw new InvalidOperationException("Reply was called but we have no current message to reply to.");

            if (currentMessage.ReplyToAddress == Address.Undefined)
                throw new InvalidOperationException("Reply was called but the current message does not have a reply to address.");

            MessageRuleValidation.ValidateIsMessageType(messages);
            messageTransport.SendMessage(currentMessage.ReplyToAddress, currentMessage.CorrelationId, TimeSpan.MaxValue, messages);
        }

        public void Return<TEnum>(TEnum errorCode) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var currentMessage = messageTransport.CurrentTransportMessage;

            if (currentMessage == null || currentMessage == TransportMessage.Undefined)
                throw new InvalidOperationException("Return was called but we have no current message to return.");

            if (currentMessage.ReplyToAddress == Address.Undefined)
                throw new InvalidOperationException("Return was called with undefined reply-to-address field.");

            HeaderValue errorCodeHeader = HeaderValue.FromEnum(HeaderKeys.ReturnErrorCode, errorCode);

            messageTransport.SendControlMessage(currentMessage.ReplyToAddress, currentMessage.CorrelationId, errorCodeHeader);
        }

        public void Publish(params object[] messages)
        {
            MessageRuleValidation.ValidateIsEventType(messages);
            messagePublisher.Publish(messages);
        }

        public void Publish(Guid correlationId, params object[] messages)
        {
            MessageRuleValidation.ValidateIsEventType(messages);
            messagePublisher.Publish(correlationId, messages);
        }       
    }
}

