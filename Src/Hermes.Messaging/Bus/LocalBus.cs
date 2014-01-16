using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Transports;
using Hermes.Persistence;

using Microsoft.Practices.ServiceLocation;

using ServiceLocator = Hermes.Ioc.ServiceLocator;

namespace Hermes.Messaging.Bus
{
    public class LocalBus : IInMemoryBus
    {
        private readonly ITransportMessages messageTransport;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(IncomingMessageContext));
        private readonly IDispatchMessagesToHandlers dispatcher;

        private readonly ThreadLocal<bool> messageBeingProcessed = new ThreadLocal<bool>();

        public LocalBus(ITransportMessages messageTransport, IDispatchMessagesToHandlers dispatcher)
        {
            this.messageTransport = messageTransport;
            this.dispatcher = dispatcher;
        }

        public void Execute(object message)
        {
            if (message == null)
            {
                return;
            }

            if (!Settings.IsClientEndpoint)
            {
                throw new InvalidOperationException("Only a client endpoint may use IInMemoryBus to execute a command.");
            }

            if (messageBeingProcessed.Value)
            {
                throw new InvalidOperationException("Only one comand may be processed at a time. Either group all required commands together in a single Execute call or send additional commands via the message bus.");
            }

            if (messageTransport.CurrentMessage.MessageId != Guid.Empty)
            {
                throw new InvalidOperationException("A command may not be executed while an incoming message is being processed.");
            }

            try
            {
                MessageRuleValidation.ValidateIsCommandType(message);

                using (IContainer scope = Settings.RootContainer.BeginLifetimeScope())
                {
                    TryProcessMessages(message, scope);
                }
            }
            finally
            {
                messageBeingProcessed.Value = false;
            }
        }

        private void TryProcessMessages(object message, IServiceLocator serviceLocator)
        {
            var unitsOfWork = serviceLocator.GetAllInstances<IUnitOfWork>().ToArray();

            try
            {
                dispatcher.DispatchToHandlers(message, serviceLocator);
                CommitUnitsOfWork(unitsOfWork);
            }
            catch
            {
                RollBackUnitsOfWork(unitsOfWork);
                throw;
            }
        }

        private void CommitUnitsOfWork(IEnumerable<IUnitOfWork> unitsOfWork)
        {
            Logger.Verbose("Committing units of work");

            foreach (var unitOfWork in unitsOfWork.Reverse())
            {
                unitOfWork.Commit();
            }
        }

        private void RollBackUnitsOfWork(IEnumerable<IUnitOfWork> unitsOfWork)
        {
            Logger.Verbose("Rolling back units of work");

            foreach (var unitOfWork in unitsOfWork)
            {
                unitOfWork.Rollback();
            }
        }

        void IInMemoryBus.Raise(object @event)
        {
            MessageRuleValidation.ValidateIsEventType(@event);
            dispatcher.DispatchToHandlers(@event, ServiceLocator.Current);
        }
    }
}