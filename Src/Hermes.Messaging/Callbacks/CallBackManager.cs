using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Callbacks
{
    public class CallBackManager : IManageCallbacks
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(CallBackManager));

        /// <summary>
        /// Map of message IDs to Async Results - useful for cleanup in case of timeouts.
        /// </summary>
        protected readonly IDictionary<Guid, BusAsyncResult> MessageIdToAsyncResultLookup = new Dictionary<Guid, BusAsyncResult>();

        public void HandleCallback(IncomingMessageContext context)
        {
            if (context.CorrelationId == Guid.Empty)
                return;

            Logger.Debug("Handling callback for message {0} with correlation ID {1}", context.MessageId, context.CorrelationId);

            BusAsyncResult busAsyncResult;

            lock (MessageIdToAsyncResultLookup)
            {
                MessageIdToAsyncResultLookup.TryGetValue(context.CorrelationId, out busAsyncResult);
                MessageIdToAsyncResultLookup.Remove(context.CorrelationId);
            }

            if (busAsyncResult == null)
            {
                Logger.Debug("No callback is registered with correlation ID {0}", context.CorrelationId);
                return;
            }

            int statusCode = 0;

            if (context.IsControlMessage())
            {                
                HeaderValue errorCodeHeader;

                if (context.TryGetHeaderValue(HeaderKeys.ReturnErrorCode, out errorCodeHeader))
                {
                    statusCode = Int32.Parse(errorCodeHeader.Value);
                }
            }

            Logger.Debug("Calling callback for correlation ID {0}", context.CorrelationId);
            busAsyncResult.Complete(statusCode, context.Messages);
        }

        public ICallback SetupCallback(Guid correlationId)
        {
            Mandate.That<ArgumentException>(correlationId != Guid.Empty, "You must provide correlationId that is a non-empty Guid in order to perform a callback action.");

            var result = new Callback(correlationId);

            result.Registered += delegate(object sender, BusAsyncResultEventArgs args)
            {
                lock (MessageIdToAsyncResultLookup)
                {
                    Logger.Debug("Registering a callback for correlation ID {0}", correlationId);
                    MessageIdToAsyncResultLookup[args.MessageId] = args.Result;
                    args.Result.OnTimeout += Result_OnTimeout;
                }
            };

            return result;
        }

        void Result_OnTimeout(object sender, EventArgs e)
        {
            lock (MessageIdToAsyncResultLookup)
            {
                KeyValuePair<Guid, BusAsyncResult> keyValue = MessageIdToAsyncResultLookup.SingleOrDefault(pair => ReferenceEquals(pair.Value, sender));

                if (!keyValue.Equals(new KeyValuePair<Guid, BusAsyncResult>()))
                {
                    Logger.Debug("Callback timeout for correlation ID {0}", keyValue.Key);
                    MessageIdToAsyncResultLookup.Remove(keyValue);
                }

                ((BusAsyncResult)sender).OnTimeout -= Result_OnTimeout;
            }
        }
    }
}