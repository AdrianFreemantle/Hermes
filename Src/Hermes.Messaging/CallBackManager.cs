using System;
using System.Collections.Generic;

using Hermes.Messaging.BusCallback;

namespace Hermes.Messaging
{
    public class CallBackManager : ICallBackManager
    {
        /// <summary>
        /// Map of message IDs to Async Results - useful for cleanup in case of timeouts.
        /// </summary>
        protected readonly IDictionary<Guid, BusAsyncResult> MessageIdToAsyncResultLookup = new Dictionary<Guid, BusAsyncResult>();

        public void HandleCallback(TransportMessage message, IReadOnlyCollection<object> messages)
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

            var statusCode = 0;

            if (message.IsControlMessage() && message.Headers.ContainsKey(Headers.ReturnMessageErrorCodeHeader))
                statusCode = int.Parse(message.Headers[Headers.ReturnMessageErrorCodeHeader]);

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
                }
            };

            return result;
        }
    }
}