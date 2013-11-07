using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
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

        private readonly ThreadLocal<bool> messageBeingProcessed = new ThreadLocal<bool>();

        public LocalBus(ITransportMessages messageTransport)
        {
            this.messageTransport = messageTransport;
        }

        public void Execute(params object[] messages)
        {
            if (messages == null || messages.Length == 0)
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
                MessageRuleValidation.ValidateIsCommandType(messages);

                using (IContainer scope = Settings.RootContainer.BeginLifetimeScope())
                {
                    TryProcessMessages(messages, scope);
                }
            }
            finally
            {
                messageBeingProcessed.Value = false;
            }
        }

        private void TryProcessMessages(IEnumerable<object> messages, IServiceLocator serviceLocator)
        {
            var unitsOfWork = serviceLocator.GetAllInstances<IUnitOfWork>().ToArray();

            try
            {
                DispatchToHandlers(messages, serviceLocator);
                CommitUnitsOfWork(unitsOfWork);
            }
            catch
            {
                RollBackUnitsOfWork(unitsOfWork);
                throw;
            }
        }

        private void DispatchToHandlers(IEnumerable<object> messages, IServiceLocator serviceLocator)
        {
            var dispatcher = serviceLocator.GetInstance<IDispatchMessagesToHandlers>();

            foreach (var message in messages)
            {
                dispatcher.DispatchToHandlers(message, serviceLocator);
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

        void IInMemoryBus.Raise(params object[] events)
        {
            MessageRuleValidation.ValidateIsEventType(events);
            var dispatcher = ServiceLocator.Current.GetService<IDispatchMessagesToHandlers>();

            foreach (var @event in events)
            {
                dispatcher.DispatchToHandlers(@event, ServiceLocator.Current);
            }
        }
    }
}