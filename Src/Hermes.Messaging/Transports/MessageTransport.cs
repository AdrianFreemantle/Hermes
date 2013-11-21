using System;
using System.Threading;
using System.Transactions;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Configuration;

using Microsoft.Practices.ServiceLocation;

using ServiceLocator = Hermes.Ioc.ServiceLocator;

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
        private readonly IHandleMessageErrors errorHandler; 
        private readonly IManageCallbacks callBackManager;
        private readonly IContainer container;
        private readonly NullMessageContext nullMessage = new NullMessageContext();
        private readonly ThreadLocal<IMessageContext> currentMessageBeingProcessed = new ThreadLocal<IMessageContext>();

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

        public MessageTransport(IPublishMessages messagePublisher, ISendMessages messageSender, IReceiveMessages messageReceiver, IManageCallbacks callBackManager, IContainer container, IHandleMessageErrors errorHandler)
        {
            this.messagePublisher = messagePublisher;
            this.messageSender = messageSender;
            this.messageReceiver = messageReceiver;
            this.callBackManager = callBackManager;
            this.container = container;
            this.errorHandler = errorHandler;
        }

        public void Dispose()
        {
            messageReceiver.Stop();            
        }

        public void Start()
        {
            messageReceiver.Start(MessageReceived);
        }

        public void Stop()
        {
            messageReceiver.Stop();
        }

        private void MessageReceived(TransportMessage incomingMessage)
        {
            using ( IContainer childContainer = container.BeginLifetimeScope())
            {
                try
                {
                    ServiceLocator.Current.SetCurrentLifetimeScope(childContainer);
                    ProcessIncomingMessage(incomingMessage, childContainer);
                }
                finally
                {
                    currentMessageBeingProcessed.Value = nullMessage;
                    ServiceLocator.Current.SetCurrentLifetimeScope(null);                    
                }
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
                }
                catch (Exception ex)
                {
                    errorHandler.Handle(transportMessage, ex);
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

        public ICallback SendMessage(Address recipient, TimeSpan timeToLive, IOutgoingMessageContext outgoingMessageContext)
        {
            var transportMessage = outgoingMessageContext.ToTransportMessage(timeToLive);
            messageSender.Send(transportMessage, recipient);
            return callBackManager.SetupCallback(transportMessage.CorrelationId);
        }

        public bool Publish(IOutgoingMessageContext outgoingMessage)
        {
            return messagePublisher.Publish(outgoingMessage);
        }
    }
}
