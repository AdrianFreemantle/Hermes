using System;
using System.Globalization;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Timeouts;
using Hermes.Messaging.Transports;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class MessageErrorModule : IModule<IncomingMessageContext>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(MessageErrorModule));
        protected readonly IPersistTimeouts TimeoutStore;
        protected readonly ISendMessages MessageSender;

        public MessageErrorModule(IPersistTimeouts timeoutStore, ISendMessages messageSender)
        {            
            TimeoutStore = timeoutStore;
            MessageSender = messageSender;
        }

        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            try
            {
                Logger.Verbose("Starting processing chain for message {0}", input);
                next();
                Logger.Verbose("Completed processing chain for message {0}", input);
            }
            catch(Exception ex)
            {
                HandleError(input, ex);
                return false;
            }

            return true;
        }

        private void HandleError(IncomingMessageContext input, Exception ex)
        {
            int firstLevelRetryCount = GetRetryCount(input, HeaderKeys.FirstLevelRetryCount);
            int secondLevelRetryCount = GetRetryCount(input, HeaderKeys.SecondLevelRetryCount);

            if (firstLevelRetryCount >= Settings.FirstLevelRetryAttempts && secondLevelRetryCount >= Settings.SecondLevelRetryAttempts)
            {
                SendToErrorQueue(input.TransportMessage, ex);
            }
            else if (firstLevelRetryCount < Settings.FirstLevelRetryAttempts)
            {
                FirstLevelRetry(input.TransportMessage, ++firstLevelRetryCount, ex);
            }
            else
            {
                SecondLevelRetry(input.TransportMessage, ++secondLevelRetryCount, ex);
            }
        }

        protected virtual int GetRetryCount(IncomingMessageContext input, string header)
        {
            HeaderValue retryHeader;

            if (input.TryGetHeaderValue(header, out retryHeader))
            {
                return Int32.Parse(retryHeader.Value);
            }

            return 0;
        }

        protected virtual void FirstLevelRetry(TransportMessage transportMessage, int retryCount, Exception ex)
        {
            Logger.Warn("First level retry on message {0} : attempt {1} {2}", transportMessage.MessageId, retryCount, ex.GetFullExceptionMessage());
            transportMessage.Headers[HeaderKeys.FirstLevelRetryCount] = (retryCount).ToString(CultureInfo.InvariantCulture);
            MessageSender.Send(transportMessage, Address.Local);
        }

        protected virtual void SecondLevelRetry(TransportMessage transportMessage, int retryCount, Exception ex)
        {
            Logger.Warn("Second level retry on message {0} : attempt {1} {2}", transportMessage.MessageId, retryCount, ex.GetFullExceptionMessage());
            transportMessage.Headers[HeaderKeys.FirstLevelRetryCount] = (0).ToString(CultureInfo.InvariantCulture);
            transportMessage.Headers[HeaderKeys.SecondLevelRetryCount] = (retryCount).ToString(CultureInfo.InvariantCulture);
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
            Logger.Error("Processing failed for message {0}. Sending to error queue {1}", transportMessage.MessageId, ex.GetFullExceptionMessage());
            transportMessage.Headers[HeaderKeys.ProcessingEndpoint] = Address.Local.ToString();
            transportMessage.Headers[HeaderKeys.FailureDetails] = ex.GetFullExceptionMessage();
            MessageSender.Send(transportMessage, Settings.ErrorEndpoint);
        }        
    }
}