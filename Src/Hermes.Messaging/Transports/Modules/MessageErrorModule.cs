using System;
using System.Globalization;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Timeouts;
using Hermes.Pipes;

namespace Hermes.Messaging.Transports.Modules
{
    public class MessageErrorModule : IModule<IncomingMessageContext>
    {
        protected readonly IPersistTimeouts TimeoutStore;
        protected readonly ISendMessages MessageSender;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(MessageErrorModule));

        public MessageErrorModule(IPersistTimeouts timeoutStore, ISendMessages messageSender)
        {            
            TimeoutStore = timeoutStore;
            MessageSender = messageSender;
        }

        public void Invoke(IncomingMessageContext input, Action next)
        {
            try
            {
                next();                
            }
            catch(Exception ex)
            {
                HandleError(input, ex);    
            }
        }

        private void HandleError(IncomingMessageContext input, Exception ex)
        {
            int retryCount = GetRetryCount(input);

            if (retryCount >= Settings.SecondLevelRetryAttempts || Settings.IsClientEndpoint)
            {
                SendToErrorQueue(input.TransportMessage, ex);
            }
            else
            {
                retryCount++;
                SendToTimeoutStore(input.TransportMessage, retryCount);
            }
        }

        protected virtual int GetRetryCount(IncomingMessageContext input)
        {
            HeaderValue retryHeader;

            if (input.TryGetHeaderValue(HeaderKeys.RetryCount, out retryHeader))
            {
                return Int32.Parse(retryHeader.Value);
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