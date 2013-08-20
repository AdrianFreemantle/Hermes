using System.Collections.Generic;

using Hermes.Logging;

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

        public void ProcessEnvelope(MessageEnvelope envelope)
        {
            Logger.Debug("Defering message: {0}", envelope.MessageId);

            timeoutStore.Add(new TimeoutData(envelope));
        }

        public void ProcessMessages(IEnumerable<object> messageBodies)
        {
            throw new System.NotImplementedException();
        }
    }
}