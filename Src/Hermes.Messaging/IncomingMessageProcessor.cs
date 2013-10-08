using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Serialization;

namespace Hermes.Messaging
{
    public class IncomingMessageProcessor : IProcessIncomingMessages
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(IncomingMessageProcessor));

        private readonly ISerializeMessages messageSerializer;
        private readonly IDispatchMessagesToHandlers messageDispatcher;
        private readonly IManageCallbacks callBackManager;
        private readonly ICollection<IManageUnitOfWork> unitsOfWork;

        private TransportMessage transportMessage;
        private object[] messages;

        public IncomingMessageProcessor(ISerializeMessages messageSerializer, IDispatchMessagesToHandlers messageDispatcher, IManageCallbacks callBackManager, IEnumerable<IManageUnitOfWork> unitsOfWork)
        {
            this.messageSerializer = messageSerializer;
            this.messageDispatcher = messageDispatcher;
            this.callBackManager = callBackManager;
            this.unitsOfWork = unitsOfWork.ToArray();
        }

        public void ProcessTransportMessage(TransportMessage incommingTransportMessage)
        {
            Logger.Verbose("Processing transport message {0}", incommingTransportMessage.MessageId);

            transportMessage = incommingTransportMessage;
            messages = messageSerializer.Deserialize(incommingTransportMessage.Body);

            callBackManager.HandleCallback(transportMessage, messages);

            Retry.Action(TryProcessMessages, Settings.FirstLevelRetryAttempts, Settings.FirstLevelRetryDelay);
        }

        private void TryProcessMessages()
        {
            try
            {
                DispatchToHandlers();
                CommitUnitsOfWork();
            }
            catch 
            {
                RollBackUnitsOfWork();
                throw;
            }
        }

        private void DispatchToHandlers()
        {
            if (transportMessage.IsControlMessage())
            {
                return;
            }

            foreach (var message in messages)
            {
                messageDispatcher.DispatchToHandlers(message);  
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