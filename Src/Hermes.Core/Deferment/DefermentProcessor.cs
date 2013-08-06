using System;
using System.Collections.Generic;
using Hermes.Logging;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Core.Deferment
{
    public class DefermentProcessor : IProcessMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(DefermentProcessor)); 

        private readonly IPersistTimeouts timeoutStore;

        public DefermentProcessor(IPersistTimeouts timeoutStore)
        {
            this.timeoutStore = timeoutStore;
        }

        public void Process(MessageEnvelope envelope)
        {
            Logger.Debug("Defering message: {0}", envelope.MessageId);

            timeoutStore.Add(new TimeoutData(envelope));
        }

        public void DispatchToHandlers(IEnumerable<object> messageBodies)
        {
            throw new NotSupportedException("The deferment message-processor does not support the processing of message bodies. Please submit the message body wrapped in a MessageEnvelope.");
        }
    }
}