using System;
using System.Globalization;

using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Storage;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    public class ErrorHandler : IHandleMessageErrors
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(IncomingMessageProcessor));
        private readonly IPersistTimeouts timeoutStore;
        
        private readonly ISendMessages messageSender;

        public ErrorHandler(IPersistTimeouts timeoutStore, ISendMessages messageSender)
        {
            this.timeoutStore = timeoutStore;
            this.messageSender = messageSender;
        }

        public void Handle(TransportMessage transportMessage, Exception ex)
        {
            int retryCount = GetRetryCount(transportMessage);

            if (++retryCount > Settings.SecondLevelRetryAttempts)
            {
                SendToErrorQueue(transportMessage, ex);
            }
            else
            {
                SendToRetryQueue(transportMessage, retryCount);
            }
        }

        private static int GetRetryCount(TransportMessage envelope)
        {
            if (envelope.Headers.ContainsKey(Headers.RetryCount))
            {
                return Int32.Parse(envelope.Headers[Headers.RetryCount]);
            }

            return 0;
        }

        private void SendToRetryQueue(TransportMessage transportMessage, int retryCount)
        {
            Logger.Warn("Sending message {0} to retry queue: attempt {1}", transportMessage.MessageId, retryCount);
            transportMessage.Headers[Headers.RetryCount] = (retryCount).ToString(CultureInfo.InvariantCulture);
            transportMessage.Headers[Headers.TimeoutExpire] = DateTime.UtcNow.Add(Settings.SecondLevelRetryDelay).ToWireFormattedString();
            transportMessage.Headers[Headers.RouteExpiredTimeoutTo] = Address.Local.ToString();

            if (transportMessage.ReplyToAddress != Address.Undefined)
            {
                transportMessage.Headers[Headers.OriginalReplyToAddress] = transportMessage.ReplyToAddress.ToString();
            }

            Logger.Warn("Defering error message: {0}", transportMessage.MessageId);
            timeoutStore.Add(new TimeoutData(transportMessage));
        }

        private void SendToErrorQueue(TransportMessage transportMessage, Exception ex)
        {
            Logger.Error("Processing failed for message {0}. Sending to error queue : {1}", transportMessage.MessageId, ex.GetFullExceptionMessage());
            transportMessage.Headers[Headers.FailureDetails] = ex.GetFullExceptionMessage();
            messageSender.Send(transportMessage, Settings.ErrorEndpoint);
        }

        public void RemoveRetryHeaders(TransportMessage envelope)
        {
            envelope.Headers.Remove(Headers.RetryCount);
            envelope.Headers.Remove(Headers.TimeoutExpire);
            envelope.Headers.Remove(Headers.RouteExpiredTimeoutTo);
            envelope.Headers.Remove(Headers.FailureDetails);
        }
    }
}