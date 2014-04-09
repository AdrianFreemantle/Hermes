using System.Threading;
using System.Transactions;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Pipeline;
using Hermes.Pipes;

namespace Hermes.Messaging.Transports
{
    public class Transport : ITransportMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Transport)); 

        private readonly IReceiveMessages messageReceiver;
        private readonly IContainer container;
        private readonly ModulePipeFactory<IncomingMessageContext> incomingPipeline;
        private readonly ModulePipeFactory<OutgoingMessageContext> outgoingPipeline;

        private readonly ThreadLocal<IMessageContext> currentMessageBeingProcessed = new ThreadLocal<IMessageContext>();

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

        protected virtual void MessageReceived(TransportMessage transportMessage)
        {
            Logger.Debug("Message {0} with correlation Id {1} received", transportMessage.MessageId, transportMessage.CorrelationId);

            using (IContainer childContainer = container.BeginLifetimeScope())
            {
                try
                {
                    ServiceLocator.Current.SetCurrentLifetimeScope(childContainer);
                    var incomingContext = new IncomingMessageContext(transportMessage, childContainer);
                    ProcessMessage(incomingContext);                    
                }
                finally
                {
                    ServiceLocator.Current.SetCurrentLifetimeScope(null);                    
                }
            }
        }

        public virtual void ProcessMessage(IncomingMessageContext incomingContext)
        {
            try
            {
                using (var scope = StartTransactionScope())
                {
                    currentMessageBeingProcessed.Value = incomingContext;
                    incomingContext.Process(incomingPipeline);
                    scope.Complete();
                }
            }
            finally
            {
                currentMessageBeingProcessed.Value = IncomingMessageContext.Null;  
            }
        }

        protected virtual TransactionScope StartTransactionScope()
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
            
            if (currentContext.Equals(IncomingMessageContext.Null))
            {
                DispatchOutgoingMessage(outgoingMessageContext);
            }
            else
            {
                EnqueOutgoingMessage(outgoingMessageContext, currentContext);
            }
        }        

        protected virtual void EnqueOutgoingMessage(OutgoingMessageContext outgoingMessageContext, IncomingMessageContext currentContext)
        {
            outgoingMessageContext.SetUserId(currentContext.UserId);
            currentContext.Enqueue(outgoingMessageContext);
        }

        protected virtual void DispatchOutgoingMessage(OutgoingMessageContext outgoingMessageContext)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                outgoingMessageContext.SetUserId(Settings.UserIdResolver);
                outgoingMessageContext.Process(outgoingPipeline, scope);
            }
        }
    }
}
