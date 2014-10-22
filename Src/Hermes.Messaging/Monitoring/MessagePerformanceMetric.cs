using System;
using Hermes.Messaging.Pipeline;

namespace Hermes.Messaging.Monitoring
{
    public class MessagePerformanceMetric
    {
        public TimeSpan TimeToProcess { get; private set; }
        public TimeSpan TimeToDeliver { get; private set; }
        public bool Error { get; private set; }

        public MessagePerformanceMetric(DateTime receivedTime, IncomingMessageContext context)
        {
            Mandate.ParameterNotNull(context, "context");
            var headers = context.TransportMessage.Headers;

            if (headers.ContainsKey(HeaderKeys.FailureDetails))
            {
                TimeToProcess = TimeSpan.Zero;
                TimeToDeliver = TimeSpan.Zero;
                Error = true;
            }
            else
            {
                DateTime completedTime = headers[HeaderKeys.CompletedTime].ToUtcDateTime();
                DateTime sentTime = headers[HeaderKeys.SentTime].ToUtcDateTime();

                TimeToProcess = completedTime - receivedTime;
                TimeToDeliver = receivedTime - sentTime;
                Error = false;

                if (TimeToDeliver < TimeSpan.Zero)
                    TimeToDeliver = TimeSpan.Zero;

                if (TimeToProcess < TimeSpan.Zero)
                    TimeToProcess = TimeSpan.Zero;
            }
        }
    }
}