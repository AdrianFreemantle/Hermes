using System;
using System.Collections.Generic;

namespace Hermes.Messaging.Storage
{
    public class TimeoutData 
    {
        public Guid MessageId { get; set; }
        public string DestinationAddress { get; set; }
        public byte[] Body { get; set; }
        public DateTime Expires { get; set; }
        public Guid CorrelationId { get; set; }
        public IDictionary<string, string> Headers { get; set; }

        public TimeoutData()
        {
        }

        public TimeoutData(TransportMessage transportMessage)
        {
            if (!transportMessage.Headers.ContainsKey(Messaging.Headers.TimeoutExpire))
            {
                throw new InvalidOperationException("Non-timeout message arrived at the timeout manager, id:" + transportMessage.MessageId);
            }

            if (!transportMessage.Headers.ContainsKey(Messaging.Headers.RouteExpiredTimeoutTo))
            {
                throw new InvalidOperationException("No routing address provided for deferred message, id:" + transportMessage.MessageId);
            }

            DestinationAddress = transportMessage.Headers[Messaging.Headers.RouteExpiredTimeoutTo];
            MessageId = transportMessage.MessageId;
            Body = transportMessage.Body;
            Expires = transportMessage.Headers[Messaging.Headers.TimeoutExpire].ToUtcDateTime();
            CorrelationId = transportMessage.CorrelationId;
            Headers = transportMessage.Headers;
        }

        public override string ToString()
        {
            return string.Format("Timeout({0}) - Expires:{1}", MessageId, Expires);
        }

        public TransportMessage ToMessageEnvelope()
        {
            var replyToAddress = Address.Local;

            if (Headers != null && Headers.ContainsKey(Messaging.Headers.OriginalReplyToAddress))
            {
                replyToAddress = Address.Parse(Headers[Messaging.Headers.OriginalReplyToAddress]);
                Headers.Remove(Messaging.Headers.OriginalReplyToAddress);
            }

            return new TransportMessage(MessageId, CorrelationId, replyToAddress, TimeSpan.MaxValue, Headers, Body);
        }
    }
}