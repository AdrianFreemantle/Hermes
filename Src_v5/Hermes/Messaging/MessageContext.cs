using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public class MessageContext : IEquatable<IMessageContext>, IEquatable<MessageContext>, IMessageContext
    {
        public static readonly MessageContext Null;

        public object Message { get; set; }
        public string UserName { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid MessageId { get; set; }
        public Address ReplyToAddress { get; set; }
        public Address Destination { get; set; }
        public Dictionary<string, string> Headers { get; protected set; }

        static MessageContext()
        {
            Null = new MessageContext
            {
                Message = new object(),
                MessageId = Guid.Empty,
                CorrelationId = Guid.Empty,
                Destination = Address.Undefined,
                ReplyToAddress = Address.Undefined,
                UserName = String.Empty,
                Headers= new Dictionary<string, string>()
            };
        }

        public MessageContext()
        {
            Destination = Address.Undefined;
            ReplyToAddress = Address.Undefined;
            UserName = String.Empty;
            MessageId = SequentialGuid.New();
            CorrelationId = MessageId;
            Headers = new Dictionary<string, string>();
        }

        public MessageContext(object message)
            : this()
        {
            Message = message;
        }

        public override int GetHashCode()
        {
            return MessageId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MessageContext);
        }

        public virtual bool Equals(IMessageContext other)
        {
            if (null != other && other.GetType() == GetType())
            {
                return other.MessageId.Equals(MessageId);
            }

            return false;
        }

        public bool Equals(MessageContext other)
        {
            return Equals((IMessageContext)other);
        }

        public static bool operator ==(MessageContext left, MessageContext right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MessageContext left, MessageContext right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return MessageId.ToString();
        }
    }
}
