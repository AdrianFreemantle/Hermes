using System.Collections.Generic;

using Hermes.Logging;
using Hermes.Messaging.Pipeline;
using Hermes.Pipes;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Transports
{
    public class OutgoingMessageUnitOfWork
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(OutgoingMessageUnitOfWork));

        private readonly IServiceLocator serviceLocator;
        private readonly Queue<OutgoingMessageContext> outgoingMessages = new Queue<OutgoingMessageContext>();
        private readonly ModulePipeFactory<OutgoingMessageContext> outgoingPipeline;

        public OutgoingMessageUnitOfWork(ModulePipeFactory<OutgoingMessageContext> outgoingPipeline, IServiceLocator serviceLocator)
        {
            this.outgoingPipeline = outgoingPipeline;
            this.serviceLocator = serviceLocator;
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
                outgoingContext.Process(outgoingPipeline, serviceLocator);
            }
        }

        public void Clear()
        {
            outgoingMessages.Clear();
        }
    }
}