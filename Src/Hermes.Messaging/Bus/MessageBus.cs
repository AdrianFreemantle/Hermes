using System;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Pipeline;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Bus
{
    public class MessageBus : IMessageBus
    {        
        private readonly ITransportMessages messageTransport;
        private readonly IRouteMessageToEndpoint messageRouter;
        private readonly IManageCallbacks callBackManager;

        public IMessageContext CurrentMessage
        {
            get { return messageTransport.CurrentMessage; }
        }

        public MessageBus(ITransportMessages messageTransport, IRouteMessageToEndpoint messageRouter, IManageCallbacks callBackManager)
        {
            this.messageTransport = messageTransport;
            this.messageRouter = messageRouter;
            this.callBackManager = callBackManager;
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

            Address destination = messageRouter.GetDestinationFor(messages.First().GetType());
            var outgoingMessage = OutgoingMessageContext.BuildDeferredCommand(destination, correlationId, delay, messages);
            messageTransport.SendMessage(outgoingMessage);
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

            var outgoingMessage = OutgoingMessageContext.BuildCommand(address, correlationId, timeToLive, messages);
            messageTransport.SendMessage(outgoingMessage);
            return callBackManager.SetupCallback(outgoingMessage.CorrelationId);
        }

        public void Reply(params object[] messages)
        {
            var currentMessage = messageTransport.CurrentMessage;

            if (currentMessage == null || currentMessage.MessageId == Guid.Empty)
                throw new InvalidOperationException("Reply was called but we have no current message to reply to.");

            if (currentMessage.ReplyToAddress == Address.Undefined)
                throw new InvalidOperationException(
                    "Reply was called but the current message does not have a reply to address.");

            var outgoingMessage = OutgoingMessageContext.BuildReply(currentMessage, messages);
            messageTransport.SendMessage(outgoingMessage);
        }

        public void Return<TEnum>(TEnum errorCode) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var currentMessage = messageTransport.CurrentMessage;

            if (currentMessage == null || currentMessage.MessageId == Guid.Empty)
                throw new InvalidOperationException("Return was called but we have no current message to return.");

            if (currentMessage.ReplyToAddress == Address.Undefined)
                throw new InvalidOperationException("Return was called with undefined reply-to-address field.");

            var outgoingMessage = OutgoingMessageContext.BuildReturn(currentMessage, errorCode);

            messageTransport.SendMessage(outgoingMessage);
        }

        public void Publish(params object[] messages)
        {
            Publish(Guid.Empty, messages);
        }

        public void Publish(Guid correlationId, params object[] messages)
        {
            var outgoingMessage = OutgoingMessageContext.BuildEvent(correlationId, messages);
            messageTransport.SendMessage(outgoingMessage);
        }
    }
}

