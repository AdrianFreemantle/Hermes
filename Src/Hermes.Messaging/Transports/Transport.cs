using System.Threading;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Pipeline;
using Hermes.Pipes;
using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace Hermes.Messaging.Transports
{
    public class Transport : ITransportMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Transport)); 

        private readonly IReceiveMessages messageReceiver;
        private readonly IContainer container;
        private readonly ModulePipeFactory<IncomingMessageContext> incomingPipeline;
        private readonly ModulePipeFactory<OutgoingMessageContext> outgoingPipeline;

        private readonly WebSafeThreadLocal<IMessageContext> currentMessageBeingProcessed = new WebSafeThreadLocal<IMessageContext>();

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
                currentMessageBeingProcessed.Value = incomingContext;
                incomingContext.Process(incomingPipeline);
            }
            finally
            {
                currentMessageBeingProcessed.Value = IncomingMessageContext.Null;
            }
        }

        public void SendMessage(OutgoingMessageContext outgoingMessageContext)
        {
            var currentContext = (IncomingMessageContext)CurrentMessage;

            if (currentContext.Equals(IncomingMessageContext.Null))
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    Logger.Debug("Sending message {0}", outgoingMessageContext);
                    outgoingMessageContext.Process(outgoingPipeline, scope);
                }
            }
            else
            {
                Logger.Debug("Enqueuing message {0}", outgoingMessageContext);
                currentContext.Enqueue(outgoingMessageContext);
            }
        }        
    }
}
