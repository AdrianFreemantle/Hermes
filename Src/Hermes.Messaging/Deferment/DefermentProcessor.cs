using Hermes.Logging;
using Hermes.Messaging.Storage;

namespace Hermes.Messaging.Deferment
{
    public class DefermentProcessor : IProcessIncommingMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(DefermentProcessor)); 

        private readonly IPersistTimeouts timeoutStore;

        public DefermentProcessor(IPersistTimeouts timeoutStore)
        {
            this.timeoutStore = timeoutStore;
        }

        public void ProcessTransportMessage(TransportMessage transportMessage)
        {
            Logger.Debug("Defering message: {0}", transportMessage.MessageId);
            timeoutStore.Add(new TimeoutData(transportMessage));
        }
    }
}