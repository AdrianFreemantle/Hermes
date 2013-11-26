using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Messaging.Configuration;
using Hermes.Messaging.Transports;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Bus
{
    public class MessageBus : IMessageBus
    {
        private readonly ITransportMessages messageTransport;
        private readonly IRouteMessageToEndpoint messageRouter;
        private readonly IPersistTimeouts timeoutPersistence;
        private readonly IServiceLocator serviceLocator;

        public IMessageContext CurrentMessage
        {
            get { return messageTransport.CurrentMessage; }
        }

        public MessageBus(ITransportMessages messageTransport, IRouteMessageToEndpoint messageRouter, IPersistTimeouts timeoutPersistence)
        {
            this.messageTransport = messageTransport;
            this.messageRouter = messageRouter;
            this.timeoutPersistence = timeoutPersistence;
            serviceLocator = Settings.RootContainer;
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

        private ICallback SendMessages(Address address, Guid correlationId, TimeSpan timeToLive,
            params object[] messages)
        {
            MessageRuleValidation.ValidateIsCommandType(messages);

            IOutgoingMessageContext commandMessagae = BuildOutgoingMessage(correlationId, messages);
            return messageTransport.SendMessage(address, timeToLive, commandMessagae);
        }

        public void Reply(params object[] messages)
        {
            var currentMessage = messageTransport.CurrentMessage;

            if (currentMessage == null || currentMessage.MessageId == Guid.Empty)
                throw new InvalidOperationException("Reply was called but we have no current message to reply to.");

            if (currentMessage.ReplyToAddress == Address.Undefined)
                throw new InvalidOperationException(
                    "Reply was called but the current message does not have a reply to address.");

            MessageRuleValidation.ValidateIsMessageType(messages);
            IOutgoingMessageContext replyMessage = BuildOutgoingMessage(currentMessage.CorrelationId, messages);

            messageTransport.SendMessage(currentMessage.ReplyToAddress, TimeSpan.MaxValue, replyMessage);
        }

        public void Return<TEnum>(TEnum errorCode) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var currentMessage = messageTransport.CurrentMessage;

            if (currentMessage == null || currentMessage.MessageId == Guid.Empty)
                throw new InvalidOperationException("Return was called but we have no current message to return.");

            if (currentMessage.ReplyToAddress == Address.Undefined)
                throw new InvalidOperationException("Return was called with undefined reply-to-address field.");

            HeaderValue errorCodeHeader = HeaderValue.FromEnum(HeaderKeys.ReturnErrorCode, errorCode);

            IOutgoingMessageContext controlMessage = BuildOutgoingMessage(currentMessage.CorrelationId, new object[0]);
            controlMessage.AddHeader(errorCodeHeader);

            messageTransport.SendMessage(currentMessage.ReplyToAddress, TimeSpan.MaxValue, controlMessage);
        }

        public bool Publish(params object[] messages)
        {
            MessageRuleValidation.ValidateIsEventType(messages);
            return messageTransport.Publish(BuildOutgoingMessage(Guid.Empty, messages));
        }

        public bool Publish(Guid correlationId, params object[] messages)
        {
            MessageRuleValidation.ValidateIsEventType(messages);
            return messageTransport.Publish(BuildOutgoingMessage(correlationId, messages));
        }

        private IOutgoingMessageContext BuildOutgoingMessage(Guid correlationId, IEnumerable<object> messages)
        {
            var outgoingMessage = serviceLocator.GetInstance<IOutgoingMessageContext>();
            outgoingMessage.SetCorrelationId(correlationId);

            if (messages != null)
            {
                foreach (var message in messages)
                {
                    outgoingMessage.AddMessage(message);
                }
            }

            return outgoingMessage;
        }
    }
}

