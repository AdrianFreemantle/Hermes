using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Persistence;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Transports
{
    public class IncomingMessageContext : IIncomingMessageContext
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(IncomingMessageContext));

        private readonly ISerializeMessages messageSerializer;
        private readonly IManageCallbacks callBackManager;
        private readonly IDispatchMessagesToHandlers dispatcher;
        private readonly ICollection<IUnitOfWork> unitsOfWork;
        private readonly ICollection<IMutateIncomingMessages> messageMutators;
        private readonly List<HeaderValue> messageHeaders = new List<HeaderValue>();
        private TransportMessage transportMessage;
        private object[] messages;

        public Guid MessageId
        {
            get { return transportMessage.MessageId; }
        }

        public Guid CorrelationId
        {
            get { return transportMessage.CorrelationId; }
        }

        public Address ReplyToAddress
        {
            get { return transportMessage.ReplyToAddress; }
        }

        public IEnumerable<HeaderValue> Headers
        {
            get { return messageHeaders; }
        }

        public IncomingMessageContext(ISerializeMessages messageSerializer, IManageCallbacks callBackManager, IDispatchMessagesToHandlers dispatcher, IEnumerable<IUnitOfWork> unitsOfWork, IEnumerable<IMutateIncomingMessages> messageMutators)
        {
            this.messageSerializer = messageSerializer;
            this.callBackManager = callBackManager;
            this.dispatcher = dispatcher;
            this.messageMutators = messageMutators == null ? new IMutateIncomingMessages[0] : messageMutators.ToArray();
            this.unitsOfWork = unitsOfWork == null ? new IUnitOfWork[0] : unitsOfWork.ToArray();
        }

        public void Process(TransportMessage incomingMessage, IServiceLocator serviceLocator)
        {
            transportMessage = incomingMessage;

            Logger.Verbose("Processing transport message {0}", MessageId);

            ExtractMessages();

            Retry.Action(() => TryProcessMessages(serviceLocator), OnRetry, Settings.FirstLevelRetryAttempts, Settings.FirstLevelRetryDelay);

            callBackManager.HandleCallback(transportMessage, messages);
        }

        private void ExtractMessages()
        {
            messages = messageSerializer.Deserialize(transportMessage.Body);

            foreach (var message in messages)
            {
                MutateMessage(message);
            }

            foreach (var header in transportMessage.Headers)
            {
                messageHeaders.Add(new HeaderValue(header.Key, header.Value));
            }
        }

        private void MutateMessage(object message)
        {
            foreach (var mutator in messageMutators)
            {
                mutator.Mutate(message);
            }
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