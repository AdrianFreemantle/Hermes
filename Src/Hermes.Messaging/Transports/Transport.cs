using System;
using System.Threading;
using System.Transactions;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Pipeline;
using Hermes.Pipes;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Transports
{
    public class Transport : ITransportMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Transport)); 

        private readonly IReceiveMessages messageReceiver;
        private readonly IContainer container;
        private readonly ModulePipeFactory<IncomingMessageContext> incomingPipeline;
        private readonly ModulePipeFactory<OutgoingMessageContext> outgoingPipeline;

        private readonly ThreadLocal<IncomingMessageContext> currentMessageBeingProcessed = new ThreadLocal<IncomingMessageContext>();

        public IMessageContext CurrentMessage
        {
            get
            {
                return currentMessageBeingProcessed.Value ?? IncomingMessageContext.Null;
            }
        }

        public Transport(IReceiveMessages messageReceiver, IContainer container, ModulePipeFactory<IncomingMessageContext> incomingPipeline, ModulePipeFactory<OutgoingMessageContext> outgoingPipeline)
        {
            this.messageReceiver = messageReceiver;
            this.container = container;
            this.incomingPipeline = incomingPipeline;
            this.outgoingPipeline = outgoingPipeline;
        }

        public void Dispose()
        {
            Logger.Debug("Dispose called; stopping message receiver.");
            messageReceiver.Stop();            
        }

        public void Start()
        {
            if (Settings.IsSendOnly)
            {
                Logger.Debug("Skipping starting of message receiver for sendonly endpoint.");
            }
            else
            {
                Logger.Debug("Starting message receiver.");
                messageReceiver.Start(MessageReceived);
            }
        }

        public void Stop()
        {
            Logger.Debug("Stop called; stopping message receiver.");
            messageReceiver.Stop();
        }

        private void MessageReceived(TransportMessage transportMessage)
        {
            Logger.Debug("Message {0} with correlation Id {1} received", transportMessage.MessageId, transportMessage.CorrelationId);

            using (IContainer childContainer = container.BeginLifetimeScope())
            {
                try
                {
                    Ioc.ServiceLocator.Current.SetCurrentLifetimeScope(childContainer);
                    ProcessIncomingMessage(transportMessage, childContainer);                    
                }
                finally
                {
                    currentMessageBeingProcessed.Value = IncomingMessageContext.Null;                    
                    Ioc.ServiceLocator.Current.SetCurrentLifetimeScope(null);                    
                }
            }
        }

        private void ProcessIncomingMessage(TransportMessage transportMessage, IServiceLocator serviceLocator)
        {
            using (var scope = StartTransactionScope())
            {
                var incomingContext = new IncomingMessageContext(transportMessage, new OutgoingMessageUnitOfWork(outgoingPipeline), serviceLocator);
                currentMessageBeingProcessed.Value = incomingContext;
                incomingContext.Process(incomingPipeline);
                scope.Complete();
            }
        }

        private static TransactionScope StartTransactionScope()
        {
            if (Settings.UseDistributedTransaction)
            {
                Logger.Debug("Beginning a transaction scope with option[Required]");
                return TransactionScopeUtils.Begin(TransactionScopeOption.Required);
            }
            
            Logger.Debug("Beginning a transaction scope with option[Suppress]");
            return TransactionScopeUtils.Begin(TransactionScopeOption.Suppress);
        }

        public void SendMessage(OutgoingMessageContext outgoingMessageContext)
        {
            var currentContext = (IncomingMessageContext)CurrentMessage;
            
            if (currentContext == IncomingMessageContext.Null)
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    outgoingMessageContext.Process(outgoingPipeline, scope);
                }
            }
            else
            {
                currentContext.Enqueue(outgoingMessageContext);
            }
        }
    }
}
