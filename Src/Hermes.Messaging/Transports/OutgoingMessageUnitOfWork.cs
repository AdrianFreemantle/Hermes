using System.Collections.Generic;

using Hermes.Logging;
using Hermes.Messaging.Pipeline;
using Hermes.Pipes;

namespace Hermes.Messaging.Transports
{
    public class OutgoingMessageUnitOfWork
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(OutgoingMessageUnitOfWork)); 

        private readonly Queue<OutgoingMessageContext> outgoingMessages = new Queue<OutgoingMessageContext>();
        private readonly ModuleStack<OutgoingMessageContext> outgoingPipeline;

        public OutgoingMessageUnitOfWork(ModuleStack<OutgoingMessageContext> outgoingPipeline)
        {
            this.outgoingPipeline = outgoingPipeline;
        }

        public void Enqueue(OutgoingMessageContext outgoingMessageContext)
        {
            Logger.Debug("Enqueuing outgoing message {0}", outgoingMessageContext);
            outgoingMessages.Enqueue(outgoingMessageContext);
        }

        public void Commit()
        {            
            while (outgoingMessages.Count > 0)
            {
                OutgoingMessageContext outgoingContext = outgoingMessages.Dequeue();
                Logger.Debug("Sending enqueued message {0}", outgoingContext);
                outgoingContext.Process(outgoingPipeline, Ioc.ServiceLocator.Current);
            }
        }

        public void Clear()
        {
            outgoingMessages.Clear();
        }
    }
}