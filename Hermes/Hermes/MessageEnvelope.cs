using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Hermes
{
    [DataContract]
    public class EnvelopeMessage 
    {
        [DataMember(Order = 1, EmitDefaultValue = false, IsRequired = false)]
        private readonly Guid messageId;

        [DataMember(Order = 2, EmitDefaultValue = false, IsRequired = false)]
        private readonly Address returnAddress;

        [DataMember(Order = 3, EmitDefaultValue = false, IsRequired = false)]
        private readonly IDictionary<string, string> headers;

        [DataMember(Order = 4, EmitDefaultValue = false, IsRequired = false)]
        private readonly object[] messages;

        [IgnoreDataMember]
        private readonly TimeSpan timeToLive;

        [IgnoreDataMember]
        private readonly bool persistent;
       
        /// <summary>
        /// Gets the value which uniquely identifies the envelope message.
        /// </summary>
        public Guid MessageId
        {
            get { return this.messageId; }
        }

        /// <summary>
        /// Gets the address to which all replies should be directed.
        /// </summary>
        public Address ReturnAddress
        {
            get { return returnAddress; }
        }

        /// <summary>
        /// Gets the maximum amount of time the message will live prior to successful receipt.
        /// </summary>
        public TimeSpan TimeToLive
        {
            get { return timeToLive; }
        }

        /// <summary>
        /// Gets a value indicating whether the message is durably stored.
        /// </summary>
        public bool Persistent
        {
            get { return persistent; }
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
        public ICollection<object> Messages
        {
            get { return messages; }
        }

        /// <summary>
        /// Initializes a new instance of the EnvelopeMessage class.
        /// </summary>
        protected EnvelopeMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the EnvelopeMessage class.
        /// </summary>
        /// <param name="messageId">The value which uniquely identifies the envelope message.</param>
        /// <param name="returnAddress">The address to which all replies should be directed.</param>
        /// <param name="timeToLive">The maximum amount of time the message will live prior to successful receipt.</param>
        /// <param name="persistent">A value indicating whether the message is durably stored.</param>
        /// <param name="headers">The message headers which contain additional metadata about the logical messages.</param>
        /// <param name="messages">The collection of dispatched logical messages.</param>
        public EnvelopeMessage(
            Guid messageId,
            Address returnAddress,
            TimeSpan timeToLive,
            bool persistent,
            IDictionary<string, string> headers,
            params object[] messages)
        {
            this.messageId = messageId;
            this.returnAddress = returnAddress;
            this.timeToLive = timeToLive;
            this.persistent = persistent;
            this.headers = headers ?? new Dictionary<string, string>();
            this.messages = messages;
        }
    }
}
