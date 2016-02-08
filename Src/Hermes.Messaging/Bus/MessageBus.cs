using System;
using Hermes.Messaging.Callbacks;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Transports;

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

        public void Defer<T>(TimeSpan delay, T command) where T : class 
        {
            Defer(delay, Guid.Empty, command);
        }

        public void Defer<T>(TimeSpan delay, Guid correlationId, T command) where T : class 
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

        public ICallback Send<T>(T command) where T : class
        {
            Address destination = messageRouter.GetDestinationFor(command.GetType());
            return Send(destination, command);
        }

        public ICallback Send<T>(Address address, T command) where T : class
        {
            return Send(address, Guid.Empty, command);
        }

        public ICallback Send<T>(Address address, Guid corrolationId, T command) where T : class
        {
            return SendMessages(address, corrolationId, TimeSpan.MaxValue, command);
        }

        public ICallback Send<T>(Address address, Guid corrolationId, TimeSpan timeToLive, T command) where T : class
        {
            return SendMessages(address, corrolationId, timeToLive, command);
        }

        public ICallback Send<T>(Guid corrolationId, T command) where T : class
        {
            Address destination = messageRouter.GetDestinationFor(command.GetType());
            return SendMessages(destination, corrolationId, TimeSpan.MaxValue, command);
        }

        public ICallback Send<T>(Guid corrolationId, TimeSpan timeToLive, T command) where T : class
        {
            Address destination = messageRouter.GetDestinationFor(command.GetType());
            return SendMessages(destination, corrolationId, timeToLive, command);
        }

        private ICallback SendMessages<T>(Address address, Guid correlationId, TimeSpan timeToLive, T command) where T : class
        {
            var outgoingMessage = OutgoingMessageContext.BuildCommand(address, correlationId, timeToLive, command);
            messageTransport.SendMessage(outgoingMessage);
            return callBackManager.SetupCallback(outgoingMessage.CorrelationId);
        }

        public void Reply<T>(Address address, Guid corrolationId, T message) where T : class
        {
            if (corrolationId == Guid.Empty)
                throw new InvalidOperationException("Reply was called but we have an empty correlation Id.");

            if (address == Address.Undefined)
                throw new InvalidOperationException(
                    "Reply was called but an empty address was provided..");

            var outgoingMessage = OutgoingMessageContext.BuildReply(address, corrolationId, message);
            messageTransport.SendMessage(outgoingMessage);
        }

        public void Reply<T>(T message) where T : class
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

        public void Publish<T>(T @event) where T : class
        {
            Publish(Guid.Empty, @event);
        }

        public void Publish<T>(Guid correlationId, T @event) where T : class
        {
            var outgoingMessage = OutgoingMessageContext.BuildEvent(correlationId, @event);
            messageTransport.SendMessage(outgoingMessage);
        }
    }
}

