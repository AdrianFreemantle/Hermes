using System;
using System.Collections.Generic;
using System.Threading;
using System.Transactions;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Transports;
using Microsoft.Practices.ServiceLocation;
using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace Hermes.Messaging
{
    public class MessageTransport : ITransportMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(MessageTransport));

        private readonly ISendMessages messageSender;
        private readonly IReceiveMessages messageReceiver;
        private readonly ITransportMessageFactory transportMessageFactory;
        private readonly IHandleMessageErrors errorProcessor;
        private readonly IManageCallbacks callBackManager;
        private readonly IContainer container;
        
        private readonly ThreadLocal<TransportMessage> currentMessageBeingProcessed = new ThreadLocal<TransportMessage>();

        public TransportMessage CurrentTransportMessage
        {
            get
            {
                return currentMessageBeingProcessed.Value ?? TransportMessage.Undefined;
            }
        }

        public MessageTransport(ISendMessages messageSender, IReceiveMessages messageReceiver, ITransportMessageFactory transportMessageFactory, IHandleMessageErrors errorProcessor, IManageCallbacks callBackManager, IContainer container)
        {
            this.messageSender = messageSender;
            this.messageReceiver = messageReceiver;
            this.transportMessageFactory = transportMessageFactory;
            this.errorProcessor = errorProcessor;
            this.callBackManager = callBackManager;
            this.container = container;
        }

        public void Dispose()
        {
            messageReceiver.Stop();            
        }

        public void Start()
        {
            messageReceiver.Start(OnMessageReceived);
        }

        public void Stop()
        {
            messageReceiver.Stop();
        }

        public void OnMessageReceived(TransportMessage transportMessage)
        {
            using (IContainer childContainer = container.BeginLifetimeScope())
            {
                try
                {
                    ServiceLocator.Current.SetCurrentLifetimeScope(childContainer);
                    var processor = childContainer.GetInstance<IncomingMessageProcessor>();
                    currentMessageBeingProcessed.Value = transportMessage;
                    ProcessIncommingMessage(transportMessage, processor, childContainer);
                    currentMessageBeingProcessed.Value = TransportMessage.Undefined;
                }
                finally
                {
                    ServiceLocator.Current.SetCurrentLifetimeScope(null);                    
                }
            }
        }

        private void ProcessIncommingMessage(TransportMessage transportMessage, IncomingMessageProcessor processor, IServiceLocator serviceProvider)
        {
            using (var scope = StartTransactionScope())
            {
                try
                {
                    processor.ProcessTransportMessage(transportMessage);
                }
                catch (Exception ex)
                {
                    errorProcessor.Handle(transportMessage, ex);
                }

                scope.Complete();
            }
        }
       
        private static TransactionScope StartTransactionScope()
        {
            return Settings.UseDistributedTransaction
                ? TransactionScopeUtils.Begin(TransactionScopeOption.Required)
                : TransactionScopeUtils.Begin(TransactionScopeOption.Suppress);
        }

        public ICallback SendMessage(Address recipient, Guid correlationId, TimeSpan timeToLive, object[] messages)
        {
            return SendMessage(recipient, correlationId, timeToLive, messages, new Dictionary<string, string>());
        }

        public ICallback SendMessage(Address recipient, Guid correlationId, TimeSpan timeToLive, object[] messages, IDictionary<string, string> headers)
        {
            if (messages == null || messages.Length == 0)
                throw new InvalidOperationException("Cannot send an empty set of messages.");

            var transportMessage = transportMessageFactory.BuildTransportMessage(correlationId, timeToLive, messages);          
            Send(transportMessage, recipient);
            return callBackManager.SetupCallback(transportMessage.CorrelationId);
        }

        public void SendControlMessage(Address recipient, Guid correlationId, params HeaderValue[] headerValues)
        {
            if (headerValues == null || headerValues.Length == 0)
                throw new InvalidOperationException("Cannot send an control message without any control headers.");

            var transportMessage = transportMessageFactory.BuildControlMessage(correlationId, headerValues);
            Send(transportMessage, recipient);
        }

        private void Send(TransportMessage transportMessage, Address recipient)
        {
            if (Settings.IsClientEndpoint || currentMessageBeingProcessed.Value == null || currentMessageBeingProcessed.Value == TransportMessage.Undefined)
            {
                messageSender.Send(transportMessage, recipient);
            }
            else
            {
                var outgoingMessageManager = ServiceLocator.Current.GetService<IProcessOutgoingMessages>();
                outgoingMessageManager.Add(new OutgoingMessage(transportMessage, recipient));
            }
        }
    }
}
