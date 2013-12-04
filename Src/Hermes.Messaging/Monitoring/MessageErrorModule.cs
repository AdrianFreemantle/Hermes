using System;
using System.Globalization;

using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Timeouts;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Monitoring
{
    public class MessageErrorModule : IModule
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(MessageErrorModule));

        protected readonly IPersistTimeouts TimeoutStore;
        protected readonly ISendMessages MessageSender;

        public MessageErrorModule(ITransportMessages messageTransport, IPersistTimeouts timeoutStore, ISendMessages messageSender)
        {            
            TimeoutStore = timeoutStore;
            MessageSender = messageSender;

            messageTransport.OnMessageProcessingError += OnOnMessageProcessingError;
        }

        protected virtual void OnOnMessageProcessingError(object sender, MessageProcessingProcessingErrorEventArgs e)
        {
            int retryCount = GetRetryCount(e.TransportMessage);

            if (++retryCount > Settings.SecondLevelRetryAttempts || Settings.IsClientEndpoint)
            {
                SendToErrorQueue(e.TransportMessage, e.Error);
            }
            else
            {
                SendToTimeoutStore(e.TransportMessage, retryCount);
            }
        }

        protected virtual int GetRetryCount(TransportMessage envelope)
        {
            if (envelope.Headers.ContainsKey(HeaderKeys.RetryCount))
            {
                return Int32.Parse(envelope.Headers[HeaderKeys.RetryCount]);
            }

            return 0;
        }

        protected virtual void SendToTimeoutStore(TransportMessage transportMessage, int retryCount)
        {
            Logger.Warn("Sending message {0} to retry queue: attempt {1}", transportMessage.MessageId, retryCount);
            transportMessage.Headers[HeaderKeys.RetryCount] = (retryCount).ToString(CultureInfo.InvariantCulture);
            transportMessage.Headers[HeaderKeys.TimeoutExpire] = DateTime.UtcNow.Add(Settings.SecondLevelRetryDelay).ToWireFormattedString();
            transportMessage.Headers[HeaderKeys.RouteExpiredTimeoutTo] = Address.Local.ToString();

            if (transportMessage.ReplyToAddress != Address.Undefined)
            {
                transportMessage.Headers[HeaderKeys.OriginalReplyToAddress] = transportMessage.ReplyToAddress.ToString();
            }

            Logger.Warn("Defering error message: {0}", transportMessage.MessageId);
            TimeoutStore.Add(new TimeoutData(transportMessage));
        }

        protected virtual void SendToErrorQueue(TransportMessage transportMessage, Exception ex)
        {
            Logger.Error("Processing failed for message {0}. Sending to error queue : {1}", transportMessage.MessageId, ex.GetFullExceptionMessage());
            transportMessage.Headers[HeaderKeys.ProcessingEndpoint] = Address.Local.ToString();
            transportMessage.Headers[HeaderKeys.FailureDetails] = ex.GetFullExceptionMessage();
            MessageSender.Send(transportMessage, Settings.ErrorEndpoint);
        }      
    }
}