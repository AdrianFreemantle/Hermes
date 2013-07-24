using System;
using System.Collections.Generic;

namespace Hermes.Core.Deferment
{
    public class TimeoutData 
    {
        public Guid Id { get; set; }
        public Address Destination { get; set; }
        public byte[] State { get; set; }
        public DateTime ExpiryTime { get; set; }
        public Guid CorrelationId { get; set; }
        public IDictionary<string, string> Headers { get; set; }

        public TimeoutData()
        {
            
        }

        public TimeoutData(MessageEnvelope message)
        {
            if (!message.Headers.ContainsKey(TimeoutHeaders.Expire))
            {
                throw new InvalidOperationException("Non timeout message arrived at the timeout manager, id:" + message.MessageId);
            }

            Destination = message.Headers.ContainsKey(TimeoutHeaders.RouteExpiredTimeoutTo) 
                              ? Address.Parse(message.Headers[TimeoutHeaders.RouteExpiredTimeoutTo]) 
                              : message.ReplyToAddress;

            Id = message.MessageId;
            State = message.Body;
            ExpiryTime = message.Headers[TimeoutHeaders.Expire].ToUtcDateTime();
            CorrelationId = message.CorrelationId;
            Headers = message.Headers;
        }

        public override string ToString()
        {
            return string.Format("Timeout({0}) - Expires:{1}", Id, ExpiryTime);
        }

        public MessageEnvelope ToMessageEnvelope()
        {
            return new MessageEnvelope(Id, CorrelationId, Address.Undefined, TimeSpan.MaxValue, true, Headers, State);
        }
    }
}