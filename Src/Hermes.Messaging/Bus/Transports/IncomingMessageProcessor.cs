using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Persistence;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Bus.Transports
{
    public class IncomingMessageProcessor : IProcessIncomingMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(IncomingMessageProcessor));

        private readonly ISerializeMessages messageSerializer;
        private readonly IManageCallbacks callBackManager;
        private readonly IDispatchMessagesToHandlers dispatcher;
        private readonly ICollection<IUnitOfWork> unitsOfWork;

        private TransportMessage transportMessage;
        private object[] messages;

        public IncomingMessageProcessor(ISerializeMessages messageSerializer, IManageCallbacks callBackManager, IDispatchMessagesToHandlers dispatcher, IEnumerable<IUnitOfWork> unitsOfWork)
        {
            this.messageSerializer = messageSerializer;
            this.callBackManager = callBackManager;
            this.dispatcher = dispatcher;
            this.unitsOfWork = unitsOfWork.ToArray();
        }

        public void ProcessTransportMessage(TransportMessage incommingTransportMessage, IServiceLocator serviceLocator)
        {
            Logger.Verbose("Processing transport message {0}", incommingTransportMessage.MessageId);

            transportMessage = incommingTransportMessage;
            messages = messageSerializer.Deserialize(incommingTransportMessage.Body);

            callBackManager.HandleCallback(transportMessage, messages);

            Retry.Action(() => TryProcessMessages(serviceLocator), OnRetry, Settings.FirstLevelRetryAttempts, Settings.FirstLevelRetryDelay);
        }

        private void OnRetry(Exception ex)
        {
            Logger.Warn("Attempting first level retry for message {0} : {1}", transportMessage.MessageId, ex.Message);
        }

        private void TryProcessMessages(IServiceLocator serviceLocator)
        {
            try
            {
                DispatchToHandlers(serviceLocator);
                CommitUnitsOfWork();
            }
            catch 
            {
                RollBackUnitsOfWork();
                throw;
            }
        }

        private void DispatchToHandlers(IServiceLocator serviceLocator)
        {
            if (transportMessage.IsControlMessage())
            {
                return;
            }

            foreach (var message in messages)
            {
                dispatcher.DispatchToHandlers(message, serviceLocator);  
            }
        }

        private void CommitUnitsOfWork()
        {
            Logger.Verbose("Committing units of work");

            foreach (var unitOfWork in unitsOfWork.Reverse())
            {
                unitOfWork.Commit();
            }
        }

        private void RollBackUnitsOfWork()
        {
            Logger.Verbose("Rolling back units of work");

            foreach (var unitOfWork in unitsOfWork)
            {
                unitOfWork.Rollback();
            }
        }
    }
}