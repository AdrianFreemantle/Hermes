using System;
using System.Threading;
using System.Transactions;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Transports
{
    public delegate void MessageEventHandler(object sender, MessageProcessingEventArgs e);
    public delegate void MessageProcessingErrorEventHandler(object sender, MessageProcessingProcessingErrorEventArgs e);

    public class MessageTransport : ITransportMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(MessageTransport));

        private readonly IPublishMessages messagePublisher;
        private readonly ISendMessages messageSender;
        private readonly IReceiveMessages messageReceiver;
        private readonly IContainer container;
        private readonly NullMessageContext nullMessage = new NullMessageContext();
        
        private readonly ThreadLocal<IMessageContext> currentMessageBeingProcessed = new ThreadLocal<IMessageContext>();
        private readonly ThreadLocal<ActionCollection> outgoingMessageActions = new ThreadLocal<ActionCollection>(); 

        public event MessageEventHandler OnMessageReceived;
        public event MessageEventHandler OnMessageProcessingCompleted;
        public event MessageProcessingErrorEventHandler OnMessageProcessingError;

        public IMessageContext CurrentMessage
        {
            get
            {
                return currentMessageBeingProcessed.Value ?? nullMessage;
            }
        }

        public MessageTransport(IPublishMessages messagePublisher, ISendMessages messageSender, IReceiveMessages messageReceiver, IContainer container)
        {
            this.messagePublisher = messagePublisher;
            this.messageSender = messageSender;
            this.messageReceiver = messageReceiver;
            this.container = container;
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

        private void MessageReceived(TransportMessage incomingMessage)
        {
            using (IContainer childContainer = container.BeginLifetimeScope())
            {
                try
                {
                    InitializeOutgoingMessagesActionCollection();
                    Ioc.ServiceLocator.Current.SetCurrentLifetimeScope(childContainer);
                    ProcessIncomingMessage(incomingMessage, childContainer);
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
                try
                {
                    RaiseMessageReceivedEvent(transportMessage);
                    var incomingMessageContext = serviceLocator.GetInstance<IIncomingMessageContext>();
                    currentMessageBeingProcessed.Value = incomingMessageContext;
                    incomingMessageContext.Process(transportMessage, serviceLocator);
                    RaiseProcessingCompletedEvent(transportMessage);
                    outgoingMessageActions.Value.InvokeAll();
                }
                catch (Exception ex)
                {
                    RaiseErrorEvent(transportMessage, ex);
                }

                scope.Complete();
            }
        }

        private void RaiseMessageReceivedEvent(TransportMessage transportMessage)
        {
            if (OnMessageReceived != null)
            {
                OnMessageReceived(this, new MessageProcessingEventArgs(transportMessage));
            }
        }

        private void RaiseProcessingCompletedEvent(TransportMessage transportMessage)
        {
            if (OnMessageProcessingCompleted != null)
            {
                OnMessageProcessingCompleted(this, new MessageProcessingEventArgs(transportMessage));
            }
        }

        private void RaiseErrorEvent(TransportMessage transportMessage, Exception ex)
        {
            if (OnMessageProcessingError != null)
            {
                OnMessageProcessingError(this, new MessageProcessingProcessingErrorEventArgs(transportMessage, ex));
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
