using System;
using System.Collections.Generic;

using Hermes.Messaging;

namespace Hermes.Core.Deferment
{
    public class TimeoutData 
    {
        public Guid Id { get; set; }
        public Address Destination { get; set; }
        public byte[] Body { get; set; }
        public DateTime Expires { get; set; }
        public Guid CorrelationId { get; set; }
        public IDictionary<string, string> Headers { get; set; }

        public TimeoutData()
        {
            
        }

        public TimeoutData(MessageEnvelope message)
        {
            if (!message.Headers.ContainsKey(Core.Headers.TimeoutExpire))
            {
                throw new InvalidOperationException("Non-timeout message arrived at the timeout manager, id:" + message.MessageId);
            }

            if (!message.Headers.ContainsKey(Core.Headers.RouteExpiredTimeoutTo))
            {
                throw new InvalidOperationException("No routing address provided for deferred message, id:" + message.MessageId);
            }

            Destination = Address.Parse(message.Headers[Core.Headers.RouteExpiredTimeoutTo]);
            Id = message.MessageId;
            Body = message.Body;
            Expires = message.Headers[Core.Headers.TimeoutExpire].ToUtcDateTime();
            CorrelationId = message.CorrelationId;
            Headers = message.Headers;
        }

        public override string ToString()
        {
            return string.Format("Timeout({0}) - Expires:{1}", Id, Expires);
        }

        public MessageEnvelope ToMessageEnvelope()
        {
            return new MessageEnvelope(Id, CorrelationId, TimeSpan.MaxValue, true, Headers, Body);
        }
    }
}