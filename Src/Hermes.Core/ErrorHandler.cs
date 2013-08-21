using System;
using System.Globalization;

using Hermes.Configuration;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Transports;

namespace Hermes.Core
{
    public class ErrorHandler : IHandleMessageErrors
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Processor));
        
        private readonly ISendMessages messageSender;

        public ErrorHandler(ISendMessages messageSender)
        {
            this.messageSender = messageSender;
        }

        public void Handle(MessageEnvelope envelope, Exception ex)
        {
            int retryCount = GetRetryCount(envelope);

            if (++retryCount > Settings.SecondLevelRetryAttempts)
            {
                SendToErrorQueue(envelope, ex);
            }
            else
            {
                SendToRetryQueue(envelope, retryCount);
            }
        }

        private static int GetRetryCount(MessageEnvelope envelope)
        {
            if (envelope.Headers.ContainsKey(Headers.RetryCount))
            {
                return Int32.Parse(envelope.Headers[Headers.RetryCount]);
            }

            return 0;
        }

        private void SendToRetryQueue(MessageEnvelope envelope, int retryCount)
        {
            Logger.Warn("Sending message {0} to retry queue: attempt {1}", envelope.MessageId, retryCount);
            envelope.Headers[Headers.RetryCount] = (retryCount).ToString(CultureInfo.InvariantCulture);
            envelope.Headers[Headers.TimeoutExpire] = DateTime.UtcNow.Add(Settings.SecondLevelRetryDelay).ToWireFormattedString();
            envelope.Headers[Headers.RouteExpiredTimeoutTo] = Settings.ThisEndpoint.ToString();
            messageSender.Send(envelope, Settings.DefermentEndpoint);
        }

        private void SendToErrorQueue(MessageEnvelope envelope, Exception ex)
        {
            Logger.Error("Processing failed for message {0}. Sending to error queue : {1}", envelope.MessageId, ex.GetFullExceptionMessage());
            envelope.Headers[Headers.FailureDetails] = ex.GetFullExceptionMessage();
            messageSender.Send(envelope, Settings.ErrorEndpoint);
        }

        public void RemoveRetryHeaders(MessageEnvelope envelope)
        {
            envelope.Headers.Remove(Headers.RetryCount);
            envelope.Headers.Remove(Headers.TimeoutExpire);
            envelope.Headers.Remove(Headers.RouteExpiredTimeoutTo);
            envelope.Headers.Remove(Headers.FailureDetails);
        }
    }
}