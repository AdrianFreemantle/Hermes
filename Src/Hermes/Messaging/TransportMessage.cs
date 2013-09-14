using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Hermes.Messaging
{
    [DataContract]
    public class TransportMessage 
    {
        [DataMember(Order = 1, EmitDefaultValue = false, IsRequired = false)]
        private Guid messageId;

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
        private readonly static TransportMessage undefined = new TransportMessage(Guid.Empty, Guid.Empty, Address.Parse("__UNDEFINED"), TimeSpan.MinValue, new Dictionary<string, string>(0), new byte[0]);
       
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

        public Address ReplyToAddress
        {
            get { return replyToAddress; }
        }

        public static TransportMessage Undefined
        {
            get { return undefined; }
        }

        /// <summary>
        /// Initializes a new instance of the EnvelopeMessage class.
        /// </summary>
        protected TransportMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the EnvelopeMessage class.
        /// </summary>
        /// <param name="messageId">The value which uniquely identifies the envelope message.</param>
        /// <param name="correlationId">The unique identifier of another message bundle this message bundle is associated with.</param>
        /// <param name="replyToAddress">The address to which any reply should be sent</param>
        /// <param name="timeToLive">The maximum amount of time the message will live prior to successful receipt.</param>
        /// <param name="headers">The message headers which contain additional metadata about the logical messages.</param>
        /// <param name="body">The collection of dispatched logical messages.</param>
        public TransportMessage(Guid messageId, Guid correlationId, Address replyToAddress, TimeSpan timeToLive, IDictionary<string, string> headers, byte[] body)
        {
            this.messageId = messageId;
            this.timeToLive = timeToLive;
            this.headers = headers ?? new Dictionary<string, string>();
            this.body = body ?? new byte[0];
            this.correlationId = correlationId == Guid.Empty ? messageId : correlationId;
            this.replyToAddress = replyToAddress;
        }

        /// <summary>
        /// Initializes a new instance of the EnvelopeMessage class.
        /// </summary>
        /// <param name="messageId">The value which uniquely identifies the envelope message.</param>
        /// /// <param name="correlationId">The unique identifier of another message bundle this message bundle is associated with.</param>
        /// <param name="body">The collection of dispatched logical messages.</param>
        public TransportMessage(Guid messageId, Guid correlationId, byte[] body)
            : this(messageId, correlationId, Address.Local, TimeSpan.MaxValue, new Dictionary<string, string>(), body)
        {
        }

        /// <summary>
        /// Initializes a new instance of the EnvelopeMessage class.
        /// </summary>
        /// <param name="messageId">The value which uniquely identifies the envelope message.</param>
        /// <param name="body">The collection of dispatched logical messages.</param>
        public TransportMessage(Guid messageId, byte[] body)
            : this(messageId, messageId, body)
        {
        }

        /// <summary>
        /// Initializes a new instance of the EnvelopeMessage class.
        /// </summary>
        /// <param name="messageId">The value which uniquely identifies the envelope message.</param>
        /// <param name="correlationId">The unique identifier of another message bundle this message bundle is associated with.</param>
        /// <param name="headers">The message headers which contain additional metadata about the logical messages.</param>
        public TransportMessage(Guid messageId, Guid correlationId, IDictionary<string, string> headers)
            : this(messageId, correlationId, Address.Local, TimeSpan.MaxValue, headers, new byte[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the EnvelopeMessage class.
        /// </summary>
        /// <param name="body">The collection of dispatched logical messages.</param>
        public TransportMessage(byte[] body)
            : this(IdentityFactory.NewComb(), body)
        {
        }
      
        public void ChangeMessageId(Guid id)
        {
            messageId = id;
        }        

        public bool IsControlMessage()
        {
            return Headers != null && Headers.ContainsKey(Hermes.Headers.ControlMessageHeader);
        }
    }
}
