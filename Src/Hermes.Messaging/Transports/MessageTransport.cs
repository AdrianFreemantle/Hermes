using System;
using System.Threading;
using System.Transactions;

using Hermes.Ioc;
using Hermes.Messaging.Configuration;
using Hermes.Pipes;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Transports
{
    public class MessageTransport : ITransportMessages
    {
        private readonly IPublishMessages messagePublisher;
        private readonly ISendMessages messageSender;
        private readonly IReceiveMessages messageReceiver;
        private readonly IContainer container;
        private readonly ModuleStack<IncomingMessageContext> incommingPipeline;
        private readonly NullMessageContext nullMessage = new NullMessageContext();
        
        private readonly ThreadLocal<IMessageContext> currentMessageBeingProcessed = new ThreadLocal<IMessageContext>();
        private readonly ThreadLocal<ActionCollection> outgoingMessageActions = new ThreadLocal<ActionCollection>(); 

        public IMessageContext CurrentMessage
        {
            get
            {
                return currentMessageBeingProcessed.Value ?? nullMessage;
            }
        }

        public MessageTransport(IPublishMessages messagePublisher, ISendMessages messageSender, IReceiveMessages messageReceiver, IContainer container, ModuleStack<IncomingMessageContext> incommingPipeline)
        {
            this.messagePublisher = messagePublisher;
            this.messageSender = messageSender;
            this.messageReceiver = messageReceiver;
            this.container = container;
            this.incommingPipeline = incommingPipeline;
        }

        public void Dispose()
        {
            messageReceiver.Stop();            
        }

        public void Start()
        {
            if (!Settings.IsSendOnly)
            {
                messageReceiver.Start(MessageReceived);
            }
        }

        public void Stop()
        {
            messageReceiver.Stop();
        }

        private void MessageReceived(TransportMessage transportMessage)
        {
            using (IContainer childContainer = container.BeginLifetimeScope())
            {
                try
                {
                    InitializeOutgoingMessagesActionCollection();
                    Ioc.ServiceLocator.Current.SetCurrentLifetimeScope(childContainer);
                    ProcessIncomingMessage(transportMessage, childContainer);
                }
                finally
                {
                    currentMessageBeingProcessed.Value = nullMessage;
                    Ioc.ServiceLocator.Current.SetCurrentLifetimeScope(null);                    
                }
            }
        }

        private void InitializeOutgoingMessagesActionCollection()
        {
            if (outgoingMessageActions.Value == null)
            {
                outgoingMessageActions.Value = new ActionCollection();
            }
            else
            {
                outgoingMessageActions.Value.Clear();
            }
        }

        private void ProcessIncomingMessage(TransportMessage transportMessage, IServiceLocator serviceLocator)
        {
            using (var scope = StartTransactionScope())
            {
                var incomingContext = new IncomingMessageContext(transportMessage, serviceLocator);
                currentMessageBeingProcessed.Value = incomingContext;
                var processChain = incommingPipeline.ToProcessChain(serviceLocator);
                processChain.Invoke(incomingContext);
                outgoingMessageActions.Value.InvokeAll();

                scope.Complete();
            }
        }

        private static TransactionScope StartTransactionScope()
        {
            return Settings.UseDistributedTransaction
                ? TransactionScopeUtils.Begin(TransactionScopeOption.Required)
                : TransactionScopeUtils.Begin(TransactionScopeOption.Suppress);
        }       

        public void SendMessage(Address recipient, TimeSpan timeToLive, IOutgoingMessageContext outgoingMessageContext)
        {
            var transportMessage = outgoingMessageContext.ToTransportMessage(timeToLive);

            if (CurrentMessage.MessageId == Guid.Empty)
            {
                messageSender.Send(transportMessage, recipient);
            }
            else
            {                
                outgoingMessageActions.Value.AddAction(() => messageSender.Send(transportMessage, recipient));
            }
        }

        public void Publish(IOutgoingMessageContext outgoingMessage)
        {
            if (CurrentMessage.MessageId == Guid.Empty)
            {
                messagePublisher.Publish(outgoingMessage);
            }
            else
            {
                outgoingMessageActions.Value.AddAction(() => messagePublisher.Publish(outgoingMessage));
            }
        }
    }
}
