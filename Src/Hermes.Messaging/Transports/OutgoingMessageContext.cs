using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hermes.Messaging.Transports
{
    public class OutgoingMessageContext : IOutgoingMessageContext
    {
        private readonly List<HeaderValue> messageHeaders = new List<HeaderValue>();
        private readonly List<object> messages = new List<object>(); 
        private readonly ISerializeMessages messageSerializer;
        private readonly ICollection<IMutateOutgoingMessages> messageMutators;
        private Guid messageId;
        private Guid correlationId;
        private Address replyToAddress = Address.Local;

        public Guid MessageId
        {
            get { return messageId; }
        }

        public Guid CorrelationId
        {
            get { return correlationId; }
        }

        public Address ReplyToAddress
        {
            get { return replyToAddress; }
        }

        public IEnumerable<HeaderValue> Headers
        {
            get { return messageHeaders; }
        }

        public OutgoingMessageContext(ISerializeMessages messageSerializer, IEnumerable<IMutateOutgoingMessages> messageMutators)
        {
            this.messageSerializer = messageSerializer;
            this.messageMutators = messageMutators.ToArray();
            messageId = SequentialGuid.New();
            correlationId = messageId;
        }

        private void SetAsControlMessage()
        {
            messageHeaders.Add(new HeaderValue(HeaderKeys.ControlMessageHeader, true.ToString()));
        }

        public void AddMessage(object message)
        {
            foreach (var mutator in messageMutators)
            {
                mutator.Mutate(message);
            }

            AddMessageTypeToHeader(message);
            messages.Add(message);
        }

        private void AddMessageTypeToHeader(object message)
        {
            var headerValue = messageHeaders.FirstOrDefault(header => header.Key.Equals(HeaderKeys.MessageTypes));

            if (headerValue == null)
            {
                messageHeaders.Add(new HeaderValue(HeaderKeys.MessageTypes, message.GetType().FullName));
            }
            else
            {
                headerValue.AppendValue(message.GetType().FullName);
            }
        }

        public void SetMessageId(Guid id)
        {
            Mandate.ParameterNotDefaut(id, "id");
            messageId = id;
        }

        public void SetCorrelationId(Guid id)
        {
            correlationId = id;
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

        public TransportMessage ToTransportMessage()
        {
            return ToTransportMessage(TimeSpan.MaxValue);
        }

        public TransportMessage ToTransportMessage(TimeSpan timeToLive)
        {
            byte[] messageBody = null;
            
            if (messages.Any())
            {
                messageBody = messageSerializer.Serialize(messages.ToArray());
            }
            else
            {
                SetAsControlMessage();
            }

            return new TransportMessage(messageId, correlationId, replyToAddress, timeToLive, BuildMessageHeaders(), messageBody);
        }

        private Dictionary<string, string> BuildMessageHeaders()
        {
            var headers = new Dictionary<string, string>(Headers.Count() + 1)
            {
                {HeaderKeys.SentTime, DateTime.UtcNow.ToWireFormattedString()}
            };

            foreach (var messageHeader in messageHeaders)
            {
                headers[messageHeader.Key] = messageHeader.Value;
            }

            return headers;
        }  

        public IEnumerable<Type> GetMessageContracts()
        {
            IEnumerable<Type> messageTypes = messages
                .SelectMany(o => o.GetType().GetInterfaces())
                .Union(messages.Select(o => o.GetType()));
            return messageTypes;
        }
    }
}