using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging.Transports;
using Hermes.Pipes;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Pipeline
{
    public class OutgoingMessageContext
    {
        private readonly List<HeaderValue> messageHeaders = new List<HeaderValue>();
        private readonly Guid messageId;
        private object[] outgoingMessages = new object[0];
        private Guid correlationId;
        private TimeSpan timeToLive = TimeSpan.MaxValue;
        private Address replyToAddress = Address.Local;
        private Address destination = Address.Undefined;
        private Func<object[], byte[]> serializeBodyFunction;
        private Func<OutgoingMessageContext, Dictionary<string, string>> buildHeaderFunction;
        
        public MessageType OutgoingMessageType { get; protected set; }

        public TimeSpan TimeToLive { get { return timeToLive; } }

        public Guid MessageId
        {
            get { return messageId; }
        }

        public object[] OutgoingMessages { get { return outgoingMessages; } }

        public Guid CorrelationId
        {
            get { return correlationId == Guid.Empty ? MessageId : correlationId; }
        }

        public Address ReplyToAddress
        {
            get { return replyToAddress; }
        }

        public Address Destination
        {
            get { return destination; }
        }

        public IEnumerable<HeaderValue> Headers
        {
            get { return messageHeaders; }
        }

        protected OutgoingMessageContext()
        {
            messageId = SequentialGuid.New();
        }

        public static OutgoingMessageContext BuildDeferredCommand(Address address, Guid correlationId, TimeSpan delay, params object[] messages)
        {
            MessageRuleValidation.ValidateIsCommandType(messages);
            var context = new OutgoingMessageContext
            {
                correlationId = correlationId,
                OutgoingMessageType = MessageType.Command,
                outgoingMessages = messages,
                destination = address
            };

            string deliveryAddress = address.ToString();
            string timeout = DateTime.UtcNow.Add(delay).ToWireFormattedString();

            context.AddHeader(new HeaderValue(HeaderKeys.TimeoutExpire, timeout));
            context.AddHeader(new HeaderValue(HeaderKeys.RouteExpiredTimeoutTo, deliveryAddress));
            
            return context;
        }

        public static OutgoingMessageContext BuildCommand(Address address, Guid correlationId, TimeSpan timeToLive, params object[] messages)
        {
            MessageRuleValidation.ValidateIsCommandType(messages);

            var context = new OutgoingMessageContext
            {
                correlationId = correlationId,
                OutgoingMessageType = MessageType.Command,
                outgoingMessages = messages,
                destination = address,
                timeToLive = timeToLive
            };

            return context;
        }

        public static OutgoingMessageContext BuildEvent(Guid correlationId, params object[] messages)
        {
            MessageRuleValidation.ValidateIsEventType(messages);

            var context = new OutgoingMessageContext
            {
                correlationId = correlationId,
                OutgoingMessageType = MessageType.Event,
                outgoingMessages = messages
            };

            return context;
        }

        public static OutgoingMessageContext BuildReply(IMessageContext currentMessage, params object[] messages)
        {
            MessageRuleValidation.ValidateIsMessageType(messages);

            var context = new OutgoingMessageContext
            {
                correlationId = currentMessage.CorrelationId,
                destination = currentMessage.ReplyToAddress,
                OutgoingMessageType = MessageType.Reply,
                outgoingMessages = messages
            };

            return context;
        }

        public static OutgoingMessageContext BuildReturn<TEnum>(IMessageContext currentMessage, TEnum errorCode) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var context = new OutgoingMessageContext
            {
                correlationId = currentMessage.CorrelationId,
                destination = currentMessage.ReplyToAddress,
                OutgoingMessageType = MessageType.Control
            };

            context.AddHeader(HeaderValue.FromEnum(HeaderKeys.ReturnErrorCode, errorCode));
            context.AddHeader(new HeaderValue(HeaderKeys.ControlMessageHeader, true.ToString()));

            return context;
        }

        public void Process(ModuleStack<OutgoingMessageContext> outgoingPipeline, IServiceLocator serviceLocator)
        {
            var pipeline = outgoingPipeline.ToModuleChain(serviceLocator);
            pipeline.Invoke(this);
        }

        public void SetReplyAddress(Address address)
        {
            Mandate.That(address != Address.Undefined, String.Format("It is not possible to send a message to {0}", address));
            replyToAddress = address;
        }

        public void AddHeader(HeaderValue headerValue)
        {
            messageHeaders.Add(headerValue);
        }

        public IEnumerable<Type> GetMessageContracts()
        {
            IEnumerable<Type> messageTypes = outgoingMessages
                .SelectMany(o => o.GetType().GetInterfaces())
                .Union(outgoingMessages.Select(o => o.GetType()));

            return messageTypes;
        }

        public TransportMessage GetTransportMessage()
        {
            var body = serializeBodyFunction(OutgoingMessages);
            var headers = buildHeaderFunction(this);

            return new TransportMessage(MessageId, CorrelationId, ReplyToAddress, TimeToLive, headers, body);
        }

        public void MessageSerializationFunction(Func<object[], byte[]> serializeBody)
        {
            serializeBodyFunction = serializeBody;
        }

        public void BuildHeaderFunction(Func<OutgoingMessageContext, Dictionary<string, string>> buildHeader)
        {
            buildHeaderFunction = buildHeader;
        }

        public override string ToString()
        {
            return messageId.ToString();
        }

        public enum MessageType
        {
            Unknown,
            Reply,
            Command,
            Event,
            Control,
            Defer
        }
    }
}