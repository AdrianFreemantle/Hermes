using System;
using System.Collections.Generic;

using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Storage;

namespace Hermes.Core.Deferment
{
    public class DefermentProcessor : IProcessMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(DefermentProcessor)); 

        private readonly IPersistTimeouts timeoutStore;

        public event EventHandler<StartedMessageProcessingEventArgs> StartedMessageProcessing;
        public event EventHandler<CompletedMessageProcessingEventArgs> CompletedMessageProcessing;
        public event EventHandler<FailedMessageProcessingEventArgs> FailedMessageProcessing;

        public DefermentProcessor(IPersistTimeouts timeoutStore)
        {
            this.timeoutStore = timeoutStore;
        }

        public void ProcessEnvelope(TransportMessage transportMessage)
        {
            Logger.Debug("Defering message: {0}", transportMessage.MessageId);
            timeoutStore.Add(new TimeoutData(transportMessage));
        }

        public void ProcessMessages(IEnumerable<object> messageBodies)
        {
            throw new System.NotImplementedException();
        }
    }
}