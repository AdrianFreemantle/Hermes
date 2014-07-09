using System;
using Hermes.Messaging.Pipeline;

namespace Hermes.Messaging.Monitoring
{
    public class MessagePerformanceMetric
    {
        public TimeSpan TimeToProcess { get; private set; }
        public TimeSpan TimeToDeliver { get; private set; }
        public bool Error { get; private set; }

        public MessagePerformanceMetric(IncomingMessageContext context)
        {
            Mandate.ParameterNotNull(context, "context");
            var headers = context.TransportMessage.Headers;

            if (headers.ContainsKey(HeaderKeys.FailureDetails))
            {
                TimeToProcess = TimeSpan.FromTicks(0);
                TimeToDeliver = TimeSpan.FromTicks(0);
                Error = true;
            }
            else
            {
                DateTime completedTime = headers[HeaderKeys.CompletedTime].ToUtcDateTime();
                DateTime sentTime = headers[HeaderKeys.SentTime].ToUtcDateTime();
                DateTime receivedTime = headers[HeaderKeys.ReceivedTime].ToUtcDateTime();

                TimeToProcess = completedTime - receivedTime;
                TimeToDeliver = receivedTime - sentTime;
                Error = false;
            }
        }
    }
}