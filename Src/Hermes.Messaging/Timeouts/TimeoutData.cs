using System;
using System.Collections.Generic;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Timeouts
{
    public class TimeoutData : ITimeoutData
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
            if (!transportMessage.Headers.ContainsKey(HeaderKeys.TimeoutExpire))
            {
                throw new InvalidOperationException("Non-timeout message arrived at the timeout manager, id:" + transportMessage.MessageId);
            }

            if (!transportMessage.Headers.ContainsKey(HeaderKeys.RouteExpiredTimeoutTo))
            {
                throw new InvalidOperationException("No routing address provided for deferred message, id:" + transportMessage.MessageId);
            }

            DestinationAddress = transportMessage.Headers[HeaderKeys.RouteExpiredTimeoutTo];
            MessageId = transportMessage.MessageId;
            Body = transportMessage.Body;
            Expires = transportMessage.Headers[HeaderKeys.TimeoutExpire].ToUtcDateTime();
            CorrelationId = transportMessage.CorrelationId;
            Headers = transportMessage.Headers;
        }

        public override string ToString()
        {
            return string.Format("{0} - Expires:{1}", MessageId, Expires);
        }

        public static TransportMessage ToTransportmessage(ITimeoutData timeoutData)
        {
            var replyToAddress = Address.Local;

            if (timeoutData.Headers != null && timeoutData.Headers.ContainsKey(HeaderKeys.OriginalReplyToAddress))
            {
                replyToAddress = Address.Parse(timeoutData.Headers[HeaderKeys.OriginalReplyToAddress]);
                timeoutData.Headers.Remove(HeaderKeys.OriginalReplyToAddress);
            }

            return new TransportMessage(timeoutData.MessageId, timeoutData.CorrelationId, replyToAddress, TimeSpan.MaxValue, timeoutData.Headers, timeoutData.Body);
        }
    }
}