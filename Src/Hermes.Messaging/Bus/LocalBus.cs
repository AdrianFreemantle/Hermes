﻿using System;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Transports;
using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace Hermes.Messaging.Bus
{
    public class LocalBus : IInMemoryBus
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (LocalBus));

        private readonly IContainer container;
        private readonly ITransportMessages messageTransport;
        private readonly IDispatchMessagesToHandlers dispatcher;

        public LocalBus(ITransportMessages messageTransport, IContainer container, IDispatchMessagesToHandlers dispatcher)
        {
            this.messageTransport = messageTransport;
            this.dispatcher = dispatcher;
            this.container = container;
        }

        public void Execute(object command)
        {
            Mandate.ParameterNotNull(command, "command");
            MessageRuleValidation.ValidateCommand(command);

            if (messageTransport.CurrentMessage.MessageId != Guid.Empty)
                throw new InvalidOperationException("A command may not be executed while another command is being processed.");

            if (Settings.IsSendOnly || Settings.IsClientEndpoint || Settings.IsLocalEndpoint)
            {
                ProcessCommand(command); 
                return;
            }

            throw new InvalidOperationException("Only a client endpoint or local endpoint may execute a local command.");
        }

        protected virtual void ProcessCommand(object message)
        {
            Logger.Info("User [{0}] Executing : {1}", CurrentUser.GetCurrentUserName(), message);

            if (ServiceLocator.Current.IsDisposed())
            {
                ProcessCommandWithNewLifetimeScope(message);
            }
            else
            {
                ProcessCommandWithExistingLifetimeScope(message);
            }
        }

        private void ProcessCommandWithExistingLifetimeScope(object message)
        {
            var incomingContext = new IncomingMessageContext(message, ServiceLocator.Current);
            messageTransport.ProcessMessage(incomingContext);
        }

        private void ProcessCommandWithNewLifetimeScope(object message)
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

        public void Raise(object @event)
        {
            if(!ServiceLocator.Current.HasServiceProvider())
                throw new InvalidOperationException("A local event may only be raised within the context of an executing local command or received message.");

            MessageRuleValidation.ValidateEvent(@event);
            Logger.Info("Raising : {0}", @event);
            dispatcher.DispatchToHandlers(@event, ServiceLocator.Current);
        }
    }
}