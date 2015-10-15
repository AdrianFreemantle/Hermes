using System;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Pipes;

namespace Hermes.Messaging.Transports
{
    public class Transport 
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Transport));

        private readonly IContainer container;
        private readonly ModulePipeFactory<MessageContext> incomingPipelineFactory;
        private readonly ModulePipeFactory<MessageContext> outgoingPipelineFactory;

        private readonly WebSafeThreadLocal<IMessageContext> currentMessageBeingProcessed = new WebSafeThreadLocal<IMessageContext>();

        public IMessageContext CurrentMessage { get { return currentMessageBeingProcessed.Value ?? MessageContext.Null; } }

        public Transport(IContainer container, ModulePipeFactory<MessageContext> incomingPipelineFactory, ModulePipeFactory<MessageContext> outgoingPipelineFactory)
        {
            this.container = container;
            this.incomingPipelineFactory = incomingPipelineFactory;
            this.outgoingPipelineFactory = outgoingPipelineFactory;
        }

        public virtual void HandleIncommingMessage(MessageContext messageContext)
        {
            ProcessMessageContext(messageContext, DispatchIncommingMessageToPipleline);
        }

        private void DispatchIncommingMessageToPipleline(MessageContext messageContext)
        {
            try
            {
                currentMessageBeingProcessed.Value = messageContext;
                ModulePipe<MessageContext> pipeline = incomingPipelineFactory.Build();
                pipeline.Invoke(messageContext);
            }
            finally
            {
                currentMessageBeingProcessed.Value = MessageContext.Null;
            }
        }

        public void HandleOutgoingMessage(MessageContext messageContext)
        {
            if (CurrentMessage.Equals(MessageContext.Null))
            {
                ProcessMessageContext(messageContext, DispatchOutgoingMessageToPipleline);
            }
            else
            {
                //we will be in an existing lifetime scope as this only happens if we are busy processing an incomming message
                var ougoingMessageUnitOfWork = ServiceLocator.Current.GetService<OugoingMessageUnitOfWork>();
                ougoingMessageUnitOfWork.Enqueue(messageContext);
            }
        }

        private void DispatchOutgoingMessageToPipleline(MessageContext messageContext)
        {
            ModulePipe<MessageContext> pipeline = outgoingPipelineFactory.Build();
            pipeline.Invoke(messageContext);
        }

        private void ProcessMessageContext(MessageContext messageContext, Action<MessageContext> action)
        {
            if (ServiceLocator.Current.IsDisposed())
            {
                ProcessMessageContextInNewLifetimeScope(messageContext, action);
            }
            else
            {
                action(messageContext);
            }
        }

        private void ProcessMessageContextInNewLifetimeScope(MessageContext messageContext, Action<MessageContext> action)
        {
            using (IContainer childContainer = container.BeginLifetimeScope())
            {
                try
                {
                    ServiceLocator.Current.SetCurrentLifetimeScope(childContainer);
                    action(messageContext);
                }
                finally
                {
                    ServiceLocator.Current.SetCurrentLifetimeScope(null);
                }
            }
        }
    }
}
