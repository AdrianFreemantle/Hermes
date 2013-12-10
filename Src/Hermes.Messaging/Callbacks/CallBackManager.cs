using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Callbacks
{
    public class CallBackManager : IManageCallbacks
    {
        /// <summary>
        /// Map of message IDs to Async Results - useful for cleanup in case of timeouts.
        /// </summary>
        protected readonly IDictionary<Guid, BusAsyncResult> MessageIdToAsyncResultLookup = new Dictionary<Guid, BusAsyncResult>();

        public void HandleCallback(IncomingMessageContext context)
        {
            if (context.CorrelationId == Guid.Empty)
                return;

            BusAsyncResult busAsyncResult;

            lock (MessageIdToAsyncResultLookup)
            {
                MessageIdToAsyncResultLookup.TryGetValue(context.CorrelationId, out busAsyncResult);
                MessageIdToAsyncResultLookup.Remove(context.CorrelationId);
            }

            if (busAsyncResult == null)
                return;

            int statusCode = 0;

            if (context.IsControlMessage())
            {
                HeaderValue errorCodeHeader;

                if (context.TryGetHeaderValue(HeaderKeys.ReturnErrorCode, out errorCodeHeader))
                {
                    statusCode = Int32.Parse(errorCodeHeader.Value);
                }
            }

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
                    MessageIdToAsyncResultLookup.Remove(keyValue);
                }

                ((BusAsyncResult)sender).OnTimeout -= Result_OnTimeout;
            }
        }
    }
}