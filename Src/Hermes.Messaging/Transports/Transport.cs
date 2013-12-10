using System;
using System.Collections.Generic;
using System.Threading;
using System.Transactions;

using Hermes.Ioc;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Pipeline;
using Hermes.Pipes;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Transports
{
    public class Transport : ITransportMessages
    {
        private readonly IReceiveMessages messageReceiver;
        private readonly IContainer container;
        private readonly ModuleStack<IncomingMessageContext> incommingPipeline;
        private readonly ModuleStack<OutgoingMessageContext> outgoingPipeline;

        private readonly ThreadLocal<IncomingMessageContext> currentMessageBeingProcessed = new ThreadLocal<IncomingMessageContext>();

        public IMessageContext CurrentMessage
        {
            get
            {
                return currentMessageBeingProcessed.Value ?? IncomingMessageContext.Null;
            }
        }

        public Transport(IReceiveMessages messageReceiver, IContainer container, ModuleStack<IncomingMessageContext> incommingPipeline, ModuleStack<OutgoingMessageContext> outgoingPipeline)
        {
            this.messageReceiver = messageReceiver;
            this.container = container;
            this.incommingPipeline = incommingPipeline;
            this.outgoingPipeline = outgoingPipeline;
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
                var incomingContext = new IncomingMessageContext(transportMessage, serviceLocator);
                currentMessageBeingProcessed.Value = incomingContext;
                incomingContext.Process(incommingPipeline);

                scope.Complete();
            }
        }

        private static TransactionScope StartTransactionScope()
        {
            return Settings.UseDistributedTransaction
                ? TransactionScopeUtils.Begin(TransactionScopeOption.Required)
                : TransactionScopeUtils.Begin(TransactionScopeOption.Suppress);
        }

        public void SendMessage(OutgoingMessageContext outgoingMessageContext)
        {
            var currentIncommingMessage = (IncomingMessageContext)CurrentMessage;

            if (currentIncommingMessage == IncomingMessageContext.Null)
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    outgoingMessageContext.Process(outgoingPipeline, scope);
                }
            }
            else
            {
                outgoingMessageContext.Process(outgoingPipeline, currentIncommingMessage.ServiceLocator);
            }
        }
    }
}
