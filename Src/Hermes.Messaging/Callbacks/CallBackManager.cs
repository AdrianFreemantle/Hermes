using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Messaging.Bus.Transports;

namespace Hermes.Messaging.Callbacks
{
    public class CallBackManager : IManageCallbacks
    {
        /// <summary>
        /// Map of message IDs to Async Results - useful for cleanup in case of timeouts.
        /// </summary>
        protected readonly IDictionary<Guid, BusAsyncResult> MessageIdToAsyncResultLookup = new Dictionary<Guid, BusAsyncResult>();

        public void HandleCallback(TransportMessage message, object[] messages)
        {
            if (message.CorrelationId == Guid.Empty)
                return;

            BusAsyncResult busAsyncResult;

            lock (MessageIdToAsyncResultLookup)
            {
                MessageIdToAsyncResultLookup.TryGetValue(message.CorrelationId, out busAsyncResult);
                MessageIdToAsyncResultLookup.Remove(message.CorrelationId);
            }

            if (busAsyncResult == null)
                return;

            int statusCode = 0;

            if (message.IsControlMessage())
            {
                if (message.Headers.ContainsKey(HeaderKeys.ReturnErrorCode))
                {
                    statusCode = int.Parse(message.Headers[HeaderKeys.ReturnErrorCode]);
                }
            }

            busAsyncResult.Complete(statusCode, messages);
        }

        public ICallback SetupCallback(Guid messageId)
        {
            Mandate.That<ArgumentException>(messageId != Guid.Empty, "You must provide correlationId that is a non-empty Guid in order to perform a callback action.");

            var result = new Callback(messageId);

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