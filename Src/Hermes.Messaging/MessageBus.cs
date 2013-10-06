﻿using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Routing;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    public class MessageBus : IMessageBus, IStartableMessageBus, IDisposable
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof(MessageBus)); 

        private readonly ITransportMessages messageTransport;
        private readonly IRouteMessageToEndpoint messageRouter;
        private readonly IPublishMessages messagePublisher;

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

        public void Defer(TimeSpan delay, params ICommand[] messages)
        {
            Defer(delay, Guid.Empty, messages);
        }

        public void Defer(TimeSpan delay, Guid correlationId, params ICommand[] messages)
        {
            if (messages == null || messages.Length == 0)
                throw new InvalidOperationException("Cannot send an empty set of messages.");

            string destination = messageRouter.GetDestinationFor(messages.First().GetType()).ToString();
            string timeout = DateTime.UtcNow.Add(delay).ToWireFormattedString();

            var headers = new Dictionary<string, string>
            {
                {Headers.TimeoutExpire, timeout},
                {Headers.RouteExpiredTimeoutTo, destination}
            };

            messageTransport.SendMessage(Settings.DefermentEndpoint, correlationId, delay, messages, headers); 
        }

        public ICallback Send(params ICommand[] messages)
        {
            Address destination = GetDestination(messages);
            return Send(destination, messages);
        }

        public ICallback Send(Address address, params ICommand[] messages)
        {
            return Send(address, Guid.Empty, messages);
        }

        public ICallback Send(Address address, Guid corrolationId, params ICommand[] messages)
        {
            return SendMessages(address, corrolationId, TimeSpan.MaxValue, messages);
        }

        public ICallback Send(Address address, Guid corrolationId, TimeSpan timeToLive, params ICommand[] messages)
        {
            return SendMessages(address, corrolationId, timeToLive, messages);
        }

        public ICallback Send(Guid corrolationId, params ICommand[] messages)
        {
            Address destination = GetDestination(messages);
            return SendMessages(destination, corrolationId, TimeSpan.MaxValue, messages);
        }

        public ICallback Send(Guid corrolationId, TimeSpan timeToLive, params ICommand[] messages)
        {
            Address destination = GetDestination(messages);
            return SendMessages(destination, corrolationId, timeToLive, messages);
        }

        private ICallback SendMessages(Address address, Guid corrolationId, TimeSpan timeToLive, params object[] messages)
        {
            if (messages == null || messages.Length == 0)
                throw new InvalidOperationException("Cannot send an empty set of messages.");

            return messageTransport.SendMessage(address, corrolationId, timeToLive, messages);
        }

        public void Reply(params IMessage[] messages)
        {
            var currentMessage = messageTransport.CurrentTransportMessage;

            if (currentMessage == null || currentMessage == TransportMessage.Undefined)
                throw new InvalidOperationException("Reply was called but we have no current message to reply to.");

            if (currentMessage.ReplyToAddress == Address.Undefined)
                throw new InvalidOperationException("Reply was called but the current message does not have a reply to address.");

            messageTransport.SendMessage(currentMessage.ReplyToAddress, currentMessage.CorrelationId, TimeSpan.MaxValue, messages);
        }

        public void Return<TEnum>(TEnum errorCode) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var currentMessage = messageTransport.CurrentTransportMessage;

            if (currentMessage == null || currentMessage == TransportMessage.Undefined)
                throw new InvalidOperationException("Return was called but we have no current message to return.");

            if (currentMessage.ReplyToAddress == Address.Undefined)
                throw new InvalidOperationException("Return was called with undefined reply-to-address field.");

            var errorCodeHeader = HeaderValue.FromEnum(Headers.ReturnMessageErrorCodeHeader, errorCode);

            messageTransport.SendControlMessage(currentMessage.ReplyToAddress, currentMessage.CorrelationId, errorCodeHeader);
        }

        public void Publish(params IEvent[] messages)
        {
            messagePublisher.Publish(messages);
        }

        public void Publish(Guid correlationId, params IEvent[] messages)
        {
            messagePublisher.Publish(correlationId, messages);
        }       

        private Address GetDestination(params object[] messages)
        {
            if (messages == null || messages.Length == 0)
                throw new InvalidOperationException("Cannot send an empty set of messages.");

            return messageRouter.GetDestinationFor(messages.First().GetType());
        }
    }   
}

