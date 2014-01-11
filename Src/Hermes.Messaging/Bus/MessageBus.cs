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

        public void Defer(TimeSpan delay, object command)
        {
            Defer(delay, Guid.Empty, command);
        }

        public void Defer(TimeSpan delay, Guid correlationId, object command)
        {
            if (Settings.IsClientEndpoint)
            {
                throw new NotSupportedException("Client endpoints may not defer messages.");
            }

            if (command == null)
                throw new InvalidOperationException("Cannot send an null set of messages.");

            Address destination = messageRouter.GetDestinationFor(command.GetType());
            var outgoingMessage = OutgoingMessageContext.BuildDeferredCommand(destination, correlationId, delay, command);
            messageTransport.SendMessage(outgoingMessage);
        }

        public ICallback Send(object command)
        {
            Address destination = messageRouter.GetDestinationFor(command.GetType());
            return Send(destination, command);
        }

        public ICallback Send(Address address, object command)
        {
            return Send(address, Guid.Empty, command);
        }

        public ICallback Send(Address address, Guid corrolationId, object command)
        {
            return SendMessages(address, corrolationId, TimeSpan.MaxValue, command);
        }

        public ICallback Send(Address address, Guid corrolationId, TimeSpan timeToLive, object command)
        {
            return SendMessages(address, corrolationId, timeToLive, command);
        }

        public ICallback Send(Guid corrolationId, object command)
        {
            Address destination = messageRouter.GetDestinationFor(command.GetType());
            return SendMessages(destination, corrolationId, TimeSpan.MaxValue, command);
        }

        public ICallback Send(Guid corrolationId, TimeSpan timeToLive, object command)
        {
            Address destination = messageRouter.GetDestinationFor(command.GetType());
            return SendMessages(destination, corrolationId, timeToLive, command);
        }

        private ICallback SendMessages(Address address, Guid correlationId, TimeSpan timeToLive, object command)
        {
            var outgoingMessage = OutgoingMessageContext.BuildCommand(address, correlationId, timeToLive, command);
            messageTransport.SendMessage(outgoingMessage);
            return callBackManager.SetupCallback(outgoingMessage.CorrelationId);
        }

        public void Reply(object message)
        {
            var currentMessage = messageTransport.CurrentMessage;

            if (currentMessage == null || currentMessage.MessageId == Guid.Empty)
                throw new InvalidOperationException("Reply was called but we have no current message to reply to.");

            if (currentMessage.ReplyToAddress == Address.Undefined)
                throw new InvalidOperationException(
                    "Reply was called but the current message does not have a reply to address.");

            var outgoingMessage = OutgoingMessageContext.BuildReply(currentMessage, message);
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

        public void Publish(object @event)
        {
            Publish(Guid.Empty, @event);
        }

        public void Publish(Guid correlationId, object @event)
        {
            var outgoingMessage = OutgoingMessageContext.BuildEvent(correlationId, @event);
            messageTransport.SendMessage(outgoingMessage);
        }
    }
}

