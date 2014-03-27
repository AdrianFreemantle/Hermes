using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Transports;
using Hermes.Persistence;

using Microsoft.Practices.ServiceLocation;
using ServiceLocator = Hermes.Ioc.ServiceLocator;

//using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace Hermes.Messaging.Bus
{
    public class LocalBus : IInMemoryBus
    {
        private readonly IContainer container;
        private readonly ITransportMessages messageTransport;
        private readonly IDispatchMessagesToHandlers dispatcher;

        public LocalBus(ITransportMessages messageTransport, IContainer container, IDispatchMessagesToHandlers dispatcher)
        {
            this.messageTransport = messageTransport;
            this.dispatcher = dispatcher;
            this.container = container;
        }

        void IInMemoryBus.Execute(object command)
        {
            Mandate.ParameterNotNull(command, "command");

            if (!Settings.IsClientEndpoint)
            {
                throw new InvalidOperationException("Only a client endpoint may use IInMemoryBus to execute a command.");
            }

            if (messageTransport.CurrentMessage.MessageId != Guid.Empty)
            {
                throw new InvalidOperationException("A command may not be executed while another command is being processed.");
            }

            ProcessCommand(command);
        }

        protected virtual void ProcessCommand(object message)
        {
            try
            {
                using (IContainer childContainer = container.BeginLifetimeScope())
                {
                    ServiceLocator.Current.SetCurrentLifetimeScope(childContainer);
                    var incomingContext = new IncomingMessageContext(message, childContainer);
                    messageTransport.ProcessMessage(incomingContext);
                }
            }
            finally
            {
                ServiceLocator.Current.SetCurrentLifetimeScope(null); 
            }
            
        }

        void IInMemoryBus.Raise(object @event)
        {
            MessageRuleValidation.ValidateIsEventType(@event);
            dispatcher.DispatchToHandlers(@event, ServiceLocator.Current);
        }
    }
}