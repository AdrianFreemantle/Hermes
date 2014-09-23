using System;
using Hermes.Logging;
using Hermes.Messaging.Pipeline;

namespace Hermes.Messaging.Monitoring
{
    public class MessagePerformanceMetric
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(MessagePerformanceMetric));

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
                {
                    Logger.Warn("Message {0} has sent time of {1} and a received time of {2}", context.MessageId, sentTime.ToWireFormattedString(), receivedTime.ToWireFormattedString());
                    TimeToDeliver = TimeSpan.Zero;
                }

                if (TimeToProcess < TimeSpan.Zero)
                {
                    Logger.Warn("Message {0} has received time of {1} and a completed time of {2}", context.MessageId, receivedTime.ToWireFormattedString(), completedTime.ToWireFormattedString());
                    TimeToDeliver = TimeSpan.Zero;
                }
            }
        }
    }
}