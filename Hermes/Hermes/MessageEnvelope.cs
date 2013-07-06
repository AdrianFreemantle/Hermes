using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Hermes
{
    [DataContract]
    public class MessageEnvelope 
    {
        [DataMember(Order = 1, EmitDefaultValue = false, IsRequired = false)]
        private readonly Guid messageId;

        [DataMember(Order = 2, EmitDefaultValue = false, IsRequired = false)]
        private readonly Guid correlationId;  

        [DataMember(Order = 3, EmitDefaultValue = false, IsRequired = false)]
        private readonly Address replyToAddress;

        [DataMember(Order = 4, EmitDefaultValue = false, IsRequired = false)]
        private readonly IDictionary<string, string> headers;

        [DataMember(Order = 5, EmitDefaultValue = false, IsRequired = false)]
        private readonly byte[] body;

        [IgnoreDataMember]
        private readonly TimeSpan timeToLive;

        [IgnoreDataMember]
        private readonly bool recoverable;
        
        [IgnoreDataMember]
        private readonly static MessageEnvelope undefined = new MessageEnvelope(Guid.Empty, Guid.Empty, Address.Undefined, TimeSpan.MinValue, false, new Dictionary<string, string>(0), new byte[0]);
       
        /// <summary>
        /// Gets the value which uniquely identifies the envelope message.
        /// </summary>
        public Guid MessageId
        {
            get { return messageId; }
        }

        /// <summary>
        /// Gets the unique identifier of another message bundle this message bundle is associated with.
        /// </summary>
        public Guid CorrelationId
        {
            get { return correlationId; }
        }

        /// <summary>
        /// Gets the address to which all replies should be directed.
        /// </summary>
        public Address ReplyToAddress
        {
            get { return replyToAddress; }
        }

        /// <summary>
        /// Gets a value indicating whether the message is durably stored.
        /// </summary>
        public bool Recoverable
        {
            get { return recoverable; }
        }

        /// <summary>
        /// Gets the message headers which contain additional metadata about the logical messages.
        /// </summary>
        public IDictionary<string, string> Headers
        {
            get { return headers; }
        }

        /// <summary>
        /// Gets the collection of dispatched logical messages.
        /// </summary>
        public byte[] Body
        {
            get { return body; }
        }

        public DateTime ExpiryTime
        {
            get
            {
                return timeToLive == TimeSpan.MaxValue
                   ? DateTime.MaxValue
                   : DateTime.UtcNow.Add(timeToLive);
            }
        }

        public bool HasExpiryTime
        {
            get { return timeToLive != TimeSpan.MaxValue; }
        }

        public static MessageEnvelope Undefined
        {
            get { return undefined; }
        }

        /// <summary>
        /// Initializes a new instance of the EnvelopeMessage class.
        /// </summary>
        protected MessageEnvelope()
        {
        }

        /// <summary>
        /// Initializes a new instance of the EnvelopeMessage class.
        /// </summary>
        /// <param name="messageId">The value which uniquely identifies the envelope message.</param>
        /// <param name="correlationId">The unique identifier of another message bundle this message bundle is associated with.</param>
        /// <param name="replyToAddress">The address to which all replies should be directed.</param>
        /// <param name="timeToLive">The maximum amount of time the message will live prior to successful receipt.</param>
        /// <param name="recoverable">A value indicating whether the message is durably stored.</param>
        /// <param name="headers">The message headers which contain additional metadata about the logical messages.</param>
        /// <param name="body">The collection of dispatched logical messages.</param>
        public MessageEnvelope(
            Guid messageId,
            Guid correlationId,
            Address replyToAddress,
            TimeSpan timeToLive,
            bool recoverable,
            IDictionary<string, string> headers,
            byte[] body)
        {
            this.messageId = messageId;
            this.correlationId = correlationId;
            this.replyToAddress = replyToAddress;
            this.timeToLive = timeToLive;
            this.recoverable = recoverable;
            this.headers = headers ?? new Dictionary<string, string>();
            this.body = body;
        }        
    }
}
